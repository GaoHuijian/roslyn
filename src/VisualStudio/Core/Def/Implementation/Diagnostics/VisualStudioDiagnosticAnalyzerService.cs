// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.LanguageServices.Implementation.ProjectSystem;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Diagnostics
{
    [Export(typeof(IVisualStudioDiagnosticAnalyzerService))]
    internal partial class VisualStudioDiagnosticAnalyzerService : IVisualStudioDiagnosticAnalyzerService
    {
        private readonly VisualStudioWorkspaceImpl _workspace;
        private readonly IDiagnosticAnalyzerService _diagnosticService;

        [ImportingConstructor]
        public VisualStudioDiagnosticAnalyzerService(VisualStudioWorkspaceImpl workspace, IDiagnosticAnalyzerService diagnosticService)
        {
            _workspace = workspace;
            _diagnosticService = diagnosticService;
        }

        // *DO NOT DELETE*
        // This is used by Ruleset Editor from ManagedSourceCodeAnalysis.dll.
        public IReadOnlyDictionary<string, IEnumerable<DiagnosticDescriptor>> GetAllDiagnosticDescriptors(IVsHierarchy hierarchyOpt)
        {
            if (hierarchyOpt == null)
            {
                return _diagnosticService.GetAllDiagnosticDescriptors(null);
            }

            // Analyzers are only supported for C# and VB currently.
            var projectsWithHierarchy = _workspace.ProjectTracker.Projects
                .Where(p => p.Language == LanguageNames.CSharp || p.Language == LanguageNames.VisualBasic)
                .Where(p => p.Hierarchy == hierarchyOpt)
                .Select(p => _workspace.CurrentSolution.GetProject(p.Id));

            if (projectsWithHierarchy.Count() <= 1)
            {
                return _diagnosticService.GetAllDiagnosticDescriptors(projectsWithHierarchy.FirstOrDefault());
            }
            else
            {
                // Multiple workspace projects map to the same hierarchy, return a union of descriptors for all projects.
                // For example, this can happen for web projects where we create on the fly projects for aspx files.
                var descriptorsMap = ImmutableDictionary.CreateBuilder<string, IEnumerable<DiagnosticDescriptor>>();
                foreach (var project in projectsWithHierarchy)
                {
                    var newDescriptorTuples = _diagnosticService.GetAllDiagnosticDescriptors(project);
                    foreach (var kvp in newDescriptorTuples)
                    {
                        IEnumerable<DiagnosticDescriptor> existingDescriptors;
                        if (descriptorsMap.TryGetValue(kvp.Key, out existingDescriptors))
                        {
                            descriptorsMap[kvp.Key] = existingDescriptors.Concat(kvp.Value).Distinct();
                        }
                        else
                        {
                            descriptorsMap[kvp.Key] = kvp.Value;
                        }
                    }
                }

                return descriptorsMap.ToImmutable();
            }
        }
    }
}
