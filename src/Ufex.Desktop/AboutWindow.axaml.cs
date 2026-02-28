using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace Ufex.Desktop;

public class PluginAssemblyInfoRow
{
	public string AssemblyName { get; set; } = string.Empty;
	public string RootNamespace { get; set; } = string.Empty;
	public string Version { get; set; } = string.Empty;
}

public partial class AboutWindow : Window
{
	private bool _pluginInfoLoaded;

	public AboutWindow()
	{
		InitializeComponent();
		var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
		VersionTextBlock.Text = $"Version: {version}";
		Opened += OnOpened;
	}

	private void OnCloseClick(object? sender, RoutedEventArgs e)
	{
		Close();
	}

	private async void OnOpened(object? sender, EventArgs e)
	{
		if (_pluginInfoLoaded)
		{
			return;
		}

		_pluginInfoLoaded = true;
		string pluginsDirectory = Path.Combine(AppContext.BaseDirectory, "plugins");
		PluginSummaryTextBlock.Text = $"Scanning plugins in: {pluginsDirectory}";

		var pluginRows = await Task.Run(() => LoadPluginAssemblyInfo(pluginsDirectory));
		PluginDataGrid.ItemsSource = pluginRows;

		if (!Directory.Exists(pluginsDirectory))
		{
			PluginSummaryTextBlock.Text = $"Plugins directory not found: {pluginsDirectory}";
			return;
		}

		PluginSummaryTextBlock.Text = pluginRows.Count == 0
			? $"No plugin assemblies found in: {pluginsDirectory}"
			: $"Found {pluginRows.Count} plugin assembly{(pluginRows.Count == 1 ? string.Empty : "ies")} in: {pluginsDirectory}";
	}

	private static List<PluginAssemblyInfoRow> LoadPluginAssemblyInfo(string pluginsDirectory)
	{
		var rows = new List<PluginAssemblyInfoRow>();

		if (!Directory.Exists(pluginsDirectory))
		{
			return rows;
		}

		var dllPaths = Directory.GetFiles(pluginsDirectory, "*.dll", SearchOption.TopDirectoryOnly)
			.OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase);

		foreach (var dllPath in dllPaths)
		{
			rows.Add(CreatePluginAssemblyInfoRow(dllPath));
		}

		return rows;
	}

	private static PluginAssemblyInfoRow CreatePluginAssemblyInfoRow(string dllPath)
	{
		string fallbackAssemblyName = Path.GetFileNameWithoutExtension(dllPath);

		try
		{
			AssemblyName assemblyName = AssemblyName.GetAssemblyName(dllPath);
			string resolvedAssemblyName = assemblyName.Name ?? fallbackAssemblyName;
			string version = GetDisplayVersion(dllPath, assemblyName.Version);
			string rootNamespace = GetRootNamespaceFromMetadata(dllPath, resolvedAssemblyName);

			return new PluginAssemblyInfoRow
			{
				AssemblyName = resolvedAssemblyName,
				RootNamespace = rootNamespace,
				Version = version
			};
		}
		catch (Exception ex)
		{
			return new PluginAssemblyInfoRow
			{
				AssemblyName = fallbackAssemblyName,
				RootNamespace = "Unknown",
				Version = $"Error ({ex.GetType().Name})"
			};
		}
	}

	private static string GetDisplayVersion(string dllPath, Version? assemblyVersion)
	{
		string assemblyVersionText = assemblyVersion?.ToString() ?? string.Empty;
		if (!IsUnsetVersion(assemblyVersionText))
		{
			return assemblyVersionText;
		}

		string informationalVersion = GetInformationalVersionFromMetadata(dllPath);
		if (!IsUnsetVersion(informationalVersion))
		{
			return informationalVersion;
		}

		try
		{
			var fileVersionInfo = FileVersionInfo.GetVersionInfo(dllPath);
			string fileVersion = fileVersionInfo.FileVersion ?? string.Empty;
			if (!IsUnsetVersion(fileVersion))
			{
				return fileVersion;
			}

			string productVersion = fileVersionInfo.ProductVersion ?? string.Empty;
			if (!IsUnsetVersion(productVersion))
			{
				return productVersion;
			}
		}
		catch
		{
			// Ignore and use the fallback below.
		}

		return "Not set";
	}

	private static string GetInformationalVersionFromMetadata(string dllPath)
	{
		try
		{
			using var stream = File.OpenRead(dllPath);
			using var peReader = new PEReader(stream);

			if (!peReader.HasMetadata)
			{
				return string.Empty;
			}

			MetadataReader metadataReader = peReader.GetMetadataReader();
			foreach (CustomAttributeHandle attrHandle in metadataReader.GetAssemblyDefinition().GetCustomAttributes())
			{
				CustomAttribute attr = metadataReader.GetCustomAttribute(attrHandle);
				string attrTypeName = GetAttributeTypeName(metadataReader, attr);

				if (!attrTypeName.Equals("System.Reflection.AssemblyInformationalVersionAttribute", StringComparison.Ordinal) &&
					!attrTypeName.Equals("AssemblyInformationalVersionAttribute", StringComparison.Ordinal))
				{
					continue;
				}

				var valueReader = metadataReader.GetBlobReader(attr.Value);
				if (valueReader.Length == 0 || valueReader.ReadUInt16() != 1)
				{
					return string.Empty;
				}

				return valueReader.ReadSerializedString() ?? string.Empty;
			}
		}
		catch
		{
			// Ignore and return empty.
		}

		return string.Empty;
	}

	private static string GetAttributeTypeName(MetadataReader metadataReader, CustomAttribute attr)
	{
		EntityHandle ctorHandle = attr.Constructor;

		if (ctorHandle.Kind == HandleKind.MemberReference)
		{
			MemberReference memberRef = metadataReader.GetMemberReference((MemberReferenceHandle)ctorHandle);
			if (memberRef.Parent.Kind == HandleKind.TypeReference)
			{
				TypeReference typeRef = metadataReader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
				string typeNamespace = metadataReader.GetString(typeRef.Namespace);
				string typeName = metadataReader.GetString(typeRef.Name);
				return string.IsNullOrEmpty(typeNamespace) ? typeName : $"{typeNamespace}.{typeName}";
			}

			if (memberRef.Parent.Kind == HandleKind.TypeDefinition)
			{
				TypeDefinition typeDef = metadataReader.GetTypeDefinition((TypeDefinitionHandle)memberRef.Parent);
				string typeNamespace = metadataReader.GetString(typeDef.Namespace);
				string typeName = metadataReader.GetString(typeDef.Name);
				return string.IsNullOrEmpty(typeNamespace) ? typeName : $"{typeNamespace}.{typeName}";
			}
		}

		return string.Empty;
	}

	private static bool IsUnsetVersion(string? version)
	{
		if (string.IsNullOrWhiteSpace(version))
		{
			return true;
		}

		string trimmedVersion = version.Trim();
		return trimmedVersion.Equals("0.0.0.0", StringComparison.Ordinal) ||
			   trimmedVersion.Equals("0.0.0", StringComparison.Ordinal) ||
			   trimmedVersion.Equals("0.0", StringComparison.Ordinal);
	}

	private static string GetRootNamespaceFromMetadata(string dllPath, string fallbackValue)
	{
		try
		{
			using var stream = File.OpenRead(dllPath);
			using var peReader = new PEReader(stream);

			if (!peReader.HasMetadata)
			{
				return fallbackValue;
			}

			MetadataReader metadataReader = peReader.GetMetadataReader();
			var namespaces = new HashSet<string>(StringComparer.Ordinal);

			foreach (TypeDefinitionHandle typeHandle in metadataReader.TypeDefinitions)
			{
				TypeDefinition typeDef = metadataReader.GetTypeDefinition(typeHandle);
				string namespaceName = metadataReader.GetString(typeDef.Namespace);

				if (string.IsNullOrWhiteSpace(namespaceName) || namespaceName.StartsWith("<", StringComparison.Ordinal))
				{
					continue;
				}

				namespaces.Add(namespaceName);
			}

			if (namespaces.Count == 0)
			{
				return fallbackValue;
			}

			string commonPrefix = GetCommonNamespacePrefix(namespaces.OrderBy(ns => ns, StringComparer.Ordinal).ToList());
			return string.IsNullOrWhiteSpace(commonPrefix) ? fallbackValue : commonPrefix;
		}
		catch
		{
			return fallbackValue;
		}
	}

	private static string GetCommonNamespacePrefix(List<string> namespaces)
	{
		if (namespaces.Count == 0)
		{
			return string.Empty;
		}

		if (namespaces.Count == 1)
		{
			return namespaces[0];
		}

		string[] prefixParts = namespaces[0].Split('.');

		for (int i = 1; i < namespaces.Count && prefixParts.Length > 0; i++)
		{
			string[] nextParts = namespaces[i].Split('.');
			int max = Math.Min(prefixParts.Length, nextParts.Length);
			int matchLength = 0;

			while (matchLength < max &&
				   prefixParts[matchLength].Equals(nextParts[matchLength], StringComparison.Ordinal))
			{
				matchLength++;
			}

			prefixParts = prefixParts.Take(matchLength).ToArray();
		}

		return prefixParts.Length == 0 ? string.Empty : string.Join(".", prefixParts);
	}
}
