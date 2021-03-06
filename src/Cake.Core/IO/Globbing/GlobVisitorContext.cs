﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cake.Core.IO.Globbing
{
    internal sealed class GlobVisitorContext
    {
        private readonly List<string> _pathParts;
        private readonly Func<IDirectory, bool> _predicate;

        internal DirectoryPath Path { get; private set; }
        public IFileSystem FileSystem { get; }
        public ICakeEnvironment Environment { get; }
        public List<IFileSystemInfo> Results { get; }

        public GlobVisitorContext(
            IFileSystem fileSystem,
            ICakeEnvironment environment,
            Func<IDirectory, bool> predicate)
        {
            FileSystem = fileSystem;
            Environment = environment;
            _predicate = predicate;
            Results = new List<IFileSystemInfo>();
            _pathParts = new List<string>();
        }

        public void AddResult(IFileSystemInfo path)
        {
            Results.Add(path);
        }

        public void Push(string path)
        {
            _pathParts.Add(path);
            Path = GenerateFullPath();
        }

        public string Pop()
        {
            var last = _pathParts[_pathParts.Count - 1];
            _pathParts.RemoveAt(_pathParts.Count - 1);
            Path = GenerateFullPath();
            return last;
        }

        private DirectoryPath GenerateFullPath()
        {
            if (_pathParts.Count > 0 && _pathParts[0] == @"\\")
            {
                // UNC path
                var path = string.Concat(@"\\", string.Join(@"\", _pathParts.Skip(1)));
                return new DirectoryPath(path);
            }
            else
            {
                // Regular path
                var path = string.Join("/", _pathParts);
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = "./";
                }
                return new DirectoryPath(path);
            }
        }

        public bool ShouldTraverse(IDirectory info)
        {
            if (_predicate != null)
            {
                return _predicate(info);
            }
            return true;
        }
    }
}