﻿using System;
using DtpGraphCore.Model;
using DtpCore.Extensions;
using DtpGraphCore.Interfaces;
using System.Runtime.CompilerServices;
using DtpGraphCore.Extensions;
using System.Collections.Generic;
using DtpCore.Collections;
using DtpGraphCore.Enumerations;
using System.Collections.Concurrent;

namespace DtpGraphCore.Services
{
    public class GraphQueryService : IGraphQueryService
    {
        public IGraphClaimService TrustService { get; private set; }
        public long UnixTime { get; set; }

        public GraphQueryService(IGraphClaimService trustService)
        {
            TrustService = trustService;
            UnixTime = DateTime.Now.ToUnixTime();
        }

        public QueryContext Execute(QueryRequest query)
        {
            var context = new QueryContext(TrustService, query);

            if (context.Issuer != null && context.Targets.Count > 0)
                ExecuteQueryContext(context);

            return context;
        }


        protected void ExecuteQueryContext(QueryContext context)
        {
            // Keep search until the maxlevel is hit or matchlevel is hit
            while (context.Level < context.MaxLevel && context.Targets.Count > 0)
            {
                context.Level++;
                context.Visited.SetAll(false); // Reset visited

                SearchIssuer(context, context.Issuer);

                ClearTargets(context);
            }

            TrustService.BuildPackage(context);
        }


        /// <summary>
        /// Remove the targets that have been found in the last run.
        /// </summary>
        /// <param name="context"></param>
        protected void ClearTargets(QueryContext context)
        {
            foreach (var targetIssuer in context.TargetsFound.Values)
            {
                if (context.Targets.ContainsKey(targetIssuer.Index))
                    context.Targets.Remove(targetIssuer.Index);
            }
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SearchIssuer(QueryContext context, GraphIssuer issuer)
        {
            var tracker = new GraphTracker(issuer);
            context.Tracker.Push(tracker);

            // Set the Issuer to visited bit, avoiding re-searching the issuer
            context.Visited.SetFast(issuer.Index, true);

            // Process current level
            if (context.Tracker.Count == context.Level)
            {
                context.IssuerCount++;

                // Run though all targets
                foreach (var targetIndex in context.Targets.Keys)
                {
                    // Check the current issuer if it has trusted the target!
                    if (!issuer.Subjects.TryGetValue(targetIndex, out GraphSubject graphSubject))
                        continue;

                    tracker.SubjectKey = targetIndex;
                    context.SubjectCount++;

                    SearchSubject(context, tracker, graphSubject);
                }
            }
            else
            {   // Otherwise continue down!
                foreach (var subjectEntry in issuer.Subjects) // Need only to iterate subjects that are entities not things
                {
                    tracker.SubjectKey = subjectEntry.Key;

                    if (context.Visited.GetFast(subjectEntry.Value.TargetIssuer.Index))
                        continue;

                    if (FollowIssuer(context, subjectEntry.Value))
                        SearchIssuer(context, subjectEntry.Value.TargetIssuer);
                }
            }

            context.Tracker.Pop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool FollowIssuer(QueryContext context, GraphSubject subject)
        {
            if (subject.Flags.HasFlagFast(SubjectFlags.ContainsTrustTrue))
                return true;

            var follow = false;
            if (subject.Claims.GetIndex(context.ClaimScope, context.BinaryClaimTypeIndex, out int index))
                follow = (TrustService.Graph.Claims[index].Flags == ClaimFlags.Trust);

            //if (!follow) // Check Global scope disable for now. Need more expirence.
                //if (subject.Claims.GetIndex(TrustService.GlobalScopeIndex, TrustService.BinaryClaimTypeIndex, out index))
                   // follow = (TrustService.Graph.Claims[index].Flags == ClaimFlags.Trust);
            return follow;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SearchSubject(QueryContext context, GraphTracker tracker, GraphSubject subject)
        {
            int index = 0;
            var claims = GetClaims(context, subject);
            //foreach (var type in context.ClaimTypes)
            //{
            //    if (subject.Claims.GetIndex(context.ClaimScope, type, out index)) // Check local scope for claims
            //        claims.Add(new Tuple<long, int>(new SubjectClaimIndex(context.ClaimScope, type).Value, index));
            //    else
            //        if (subject.Claims.GetIndex(TrustService.GlobalScopeIndex, type, out index)) // Check global scope for claims
            //            claims.Add(new Tuple<long, int>(new SubjectClaimIndex(TrustService.GlobalScopeIndex, type).Value, index));
            //}

            if (claims.Count == 0)
                return;

            if(context.Flags == QueryFlags.IncludeClaimTrust)
                if (subject.Claims.GetIndex(context.ClaimScope, context.BinaryClaimTypeIndex, out index)) // Check local scope for claims
                    claims.Add(new Tuple<long, int>(new SubjectClaimIndex(context.ClaimScope, context.BinaryClaimTypeIndex).Value, index));
                //else // Check Global scope disable for now. Need more expirence.
                    //if (subject.Claims.GetIndex(TrustService.GlobalScopeIndex, TrustService.BinaryClaimTypeIndex, out index)) // Check global scope for claims
                       //claims.Add(new Tuple<long, int>(new SubjectClaimIndex(TrustService.GlobalScopeIndex, TrustService.BinaryClaimTypeIndex).Value, index));
            
            BuildResult(context, tracker, claims); // Target found!

            var targetIssuer = tracker.Issuer.Subjects[tracker.SubjectKey].TargetIssuer;
            context.TargetsFound[targetIssuer.Index] = targetIssuer;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected List<Tuple<long, int>> GetClaims(QueryContext context, GraphSubject subject)
        {
            var claims = new List<Tuple<long, int>>();
            int index = 0;
            foreach (var type in context.ClaimTypes)
            {
                if (subject.Claims.GetIndex(context.ClaimScope, type, out index)) // Check local scope for claims
                    claims.Add(new Tuple<long, int>(new SubjectClaimIndex(context.ClaimScope, type).Value, index));
                else
                    if (subject.Claims.GetIndex(context.GlobalScopeIndex, type, out index)) // Check global scope for claims
                    claims.Add(new Tuple<long, int>(new SubjectClaimIndex(context.GlobalScopeIndex, type).Value, index));
            }

            return claims;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void BuildResult(QueryContext context, GraphTracker currentTracker, List<Tuple<long, int>> claimsFound)
        {
            if((context.Flags & QueryFlags.LeafsOnly) == QueryFlags.LeafsOnly)
            {
                AddResult(context, currentTracker.Issuer.Index, claimsFound, currentTracker);
            }
            else
            {
                // Full tree, or first path
                foreach (var tracker in context.Tracker)
                {
                    AddResult(context, currentTracker.Issuer.Index, claimsFound, tracker);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddResult(QueryContext context, int issuerIndex, List<Tuple<long, int>> claimsFound, GraphTracker tracker)
        {
            if (!context.TrackerResults.ContainsKey(tracker.Issuer.Index))
            {
                tracker.Subjects = new GraphSubjectDictionary<int, GraphSubject>();
                context.TrackerResults.Add(tracker.Issuer.Index, tracker);
            }

            var result = context.TrackerResults[tracker.Issuer.Index];

            GraphSubject graphSubject;
            if (!result.Subjects.ContainsKey(tracker.SubjectKey))
            {   // Only subjects with unique keys
                graphSubject = tracker.Issuer.Subjects[tracker.SubjectKey]; // GraphSubject is a value type and therefore its copied
                //graphSubject.Claims = new ConcurrentDictionary<long, int>();
                graphSubject.Claims = new GraphSubjectDictionary<long, int>();
                result.Subjects.Add(tracker.SubjectKey, graphSubject);
                // Register the target found 
            } else
            {
                graphSubject = result.Subjects[tracker.SubjectKey];
            }
            
            if (graphSubject.Claims.Count == 0)
            {
                // Add claims to the current tracker level.
                var subject = tracker.Issuer.Subjects[tracker.SubjectKey];
                var claims = GetClaims(context, subject);
                foreach (var item in claims)
                {
                    result.Subjects[tracker.SubjectKey].Claims[item.Item1] = item.Item2;
                }
            }
        }
    }
}
