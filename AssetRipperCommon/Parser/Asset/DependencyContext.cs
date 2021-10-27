﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Files;
using System.Collections.Generic;

namespace AssetRipper.Core.Parser.Asset
{
	public sealed class DependencyContext
	{
		public DependencyContext(AssetLayout layout, bool log)
		{
			Layout = layout;
			IsLog = log;
			m_hierarchy = log ? new Stack<string>() : null;
		}

		public IEnumerable<PPtr<UnityObjectBase>> FetchDependencies<T>(T dependent, string name) where T : IDependent
		{
			if (IsLog)
			{
				m_hierarchy.Push(name);
			}
			foreach (PPtr<UnityObjectBase> pointer in dependent.FetchDependencies(this))
			{
				if (!pointer.IsNull)
				{
					yield return pointer;
				}
			}
			if (IsLog)
			{
				m_hierarchy.Pop();
			}
		}

		public IEnumerable<PPtr<UnityObjectBase>> FetchDependencies<T>(T[] dependents, string name) where T : IDependent
		{
			return FetchDependencies((IEnumerable<T>)dependents, name);
		}

		public IEnumerable<PPtr<UnityObjectBase>> FetchDependencies<T>(IReadOnlyList<T> dependents, string name) where T : IDependent
		{
			return FetchDependencies((IEnumerable<T>)dependents, name);
		}

		public IEnumerable<PPtr<UnityObjectBase>> FetchDependencies<T>(IEnumerable<T> dependents, string name) where T : IDependent
		{
			if (IsLog)
			{
				m_hierarchy.Push(name);
			}
			foreach (T dependent in dependents)
			{
				foreach (PPtr<UnityObjectBase> pointer in dependent.FetchDependencies(this))
				{
					if (!pointer.IsNull)
					{
						yield return pointer;
					}
				}
			}
			if (IsLog)
			{
				m_hierarchy.Pop();
			}
		}

		public IEnumerable<PPtr<UnityObjectBase>> FetchDependencies<T>(PPtr<T>[] pointers, string name) where T : UnityObjectBase
		{
			return FetchDependencies((IEnumerable<PPtr<T>>)pointers, name);
		}

		public IEnumerable<PPtr<UnityObjectBase>> FetchDependencies<T>(IReadOnlyList<PPtr<T>> pointers, string name) where T : UnityObjectBase
		{
			return FetchDependencies((IEnumerable<PPtr<T>>)pointers, name);
		}

		public IEnumerable<PPtr<UnityObjectBase>> FetchDependencies<T>(IEnumerable<PPtr<T>> pointers, string name) where T : UnityObjectBase
		{
			foreach (PPtr<T> pointer in pointers)
			{
				if (!pointer.IsNull)
				{
					yield return FetchDependency(pointer, name);
				}
			}
		}

		public PPtr<UnityObjectBase> FetchDependency<T>(PPtr<T> pointer, string name) where T : UnityObjectBase
		{
			if (IsLog)
			{
				PointerName = name;
			}
			return pointer.CastTo<UnityObjectBase>();
		}

		public string GetPointerPath()
		{
			if (m_hierarchy.Count == 0)
			{
				return string.Empty;
			}

			string hierarchy = string.Empty;
			int i = 0;
			foreach (string sub in m_hierarchy)
			{
				if (i == 0)
				{
					hierarchy = sub;
				}
				else
				{
					hierarchy = sub + "." + hierarchy;
				}
				i++;
			}
			return hierarchy;
		}

		public AssetLayout Layout { get; }
		public UnityVersion Version => Layout.Info.Version;
		public Platform Platform => Layout.Info.Platform;
		public TransferInstructionFlags Flags => Layout.Info.Flags;
		public bool IsLog { get; }
		public string PointerName { get; private set; }

		private readonly Stack<string> m_hierarchy;
	}
}