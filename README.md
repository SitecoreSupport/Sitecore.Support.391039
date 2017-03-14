# Sitecore.Support.391039
Ensures that Sitecore works smoothly even when Solr server or one of Solr cores are not available. Periodically retries to establish connection to Solr. Does not try to execute a search query if connection is not established which allows to avoid performance degradation. Alternative reference number is 94024.

[![Fixed](https://img.shields.io/badge/fixed-8.2_update--1-blue.svg)](https://dev.sitecore.net/Downloads/Sitecore%20Experience%20Platform/82/Sitecore%20Experience%20Platform%2082%20Update1/Release%20Notes)

## Main

This repository contains Sitecore Patch #391039, which extends the default `Sitecore.ContentSearch.SolrProvider.SolrSearchIndex` and `Sitecore.ContentSearch.SolrProvider.SwitchOnRebuildSolrSearchIndex` to handle unavailability of Solr server.

## Deployment

To apply the patch on either CM or CD server perform the following steps:

1. Place the `Sitecore.Support.391039.dll` assembly into the `\bin` directory.
2. Place the `Sitecore.Support.391039.config` file into the `\App_Config\Include` directory.
3. In configuration files find all occurrences of 
`Sitecore.ContentSearch.SolrProvider.SolrSearchIndex, Sitecore.ContentSearch.SolrProvider`
and replace them with
`Sitecore.Support.ContentSearch.SolrProvider.SolrSearchIndex, Sitecore.Support.391039`
4. In configuration files find all occurrences of
`Sitecore.ContentSearch.SolrProvider.SwitchOnRebuildSolrSearchIndex, Sitecore.ContentSearch.SolrProvider`
and replace them with
`Sitecore.Support.ContentSearch.SolrProvider.SwitchOnRebuildSolrSearchIndex, Sitecore.Support.391039`

## Content 

Sitecore Patch includes the following files:

1. `\bin\Sitecore.Support.391039.dll`
2. `\App_Config\Include\Sitecore.Support.391039.config`

## License

This patch is licensed under the [Sitecore Corporation A/S License](LICENSE).

## Download

Downloads are available via [GitHub Releases](https://github.com/SitecoreSupport/Sitecore.Support.391039/releases).

[![Total downloads](https://img.shields.io/github/downloads/SitecoreSupport/Sitecore.Support.391039/total.svg)](https://github.com/SitecoreSupport/Sitecore.Support.391039/releases)
