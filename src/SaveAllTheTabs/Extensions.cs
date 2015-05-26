﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using EnvDTE;
using EnvDTE80;

namespace SaveAllTheTabs
{
    internal static class Extensions
    {
        public static DocumentGroup FindByName(this IList<DocumentGroup> groups, string name)
        {
            return groups?.SingleOrDefault(g => g.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }

        public static DocumentGroup FindBySlot(this IList<DocumentGroup> groups, int index)
        {
            return groups?.SingleOrDefault(g => g.Slot == index);
        }

        public static ListViewItem GetListViewItem(this ListView list, object content)
        {
            return list.ItemContainerGenerator.ContainerFromItem(content) as ListViewItem;
        }

        public static Document GetActiveDocument(this DTE2 environment)
        {
            return environment.ActiveDocument;
        }

        public static IEnumerable<string> GetDocumentFiles(this DTE2 environment)
        {
            return from d in environment.GetDocuments() select GetExactPathName(d.FullName);
        }

        public static IEnumerable<Document> GetDocuments(this DTE2 environment)
        {
            return from w in environment.GetDocumentWindows() where w.Document != null select w.Document;
        }

        public static IEnumerable<Window> GetDocumentWindows(this DTE2 environment)
        {
            return environment.Windows.Cast<Window>().Where(x => x.Linkable == false);
        }

        public static IEnumerable<Breakpoint> GetBreakpoints(this DTE2 environment)
        {
            return environment.Debugger.Breakpoints.Cast<Breakpoint>();
        }

        public static IEnumerable<Breakpoint> GetMatchingBreakpoints(this DTE2 environment, HashSet<string> files)
        {
            return environment.Debugger.Breakpoints.Cast<Breakpoint>().Where(bp => files.Contains(bp.File));
        }

        public static void CloseAll(this IEnumerable<Window> windows)
        {
            foreach (var w in windows.Where(w => w.Document?.Saved == true))
            {
                w.Close();
            }
        }

        private static string GetExactPathName(string pathName)
        {
            if (String.IsNullOrEmpty(pathName) ||
                (!File.Exists(pathName) && !Directory.Exists(pathName)))
            {
                return pathName;
            }

            var di = new DirectoryInfo(pathName);
            return di.Parent != null
                       ? Path.Combine(GetExactPathNameCore(di.Parent),
                                      di.Parent.EnumerateFileSystemInfos(di.Name).First().Name)
                       : di.Name;
        }

        private static string GetExactPathNameCore(DirectoryInfo di)
        {
            return di.Parent != null
                       ? Path.Combine(GetExactPathNameCore(di.Parent),
                                      di.Parent.EnumerateFileSystemInfos(di.Name).First().Name)
                       : di.Name;
        }
    }
}