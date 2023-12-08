# Childrens Social Care CPD Search Indexer
This repository contains the search indexing application used within the [Childrens-Social-Care-CPD](https://github.com/DFE-Digital/childrens-social-care-cpd) website.

## Getting started
```
git clone https://github.com/DFE-Digital/childrens-social-care-cpd-indexer.git
cd src
dotnet build
```

## Running the tests
```
cd src
dotnet build
dotnet test
```

## Environment variables
The following environment variables will need to be specified for the application to run:

| Variable name  | Type/Value | Description |
| ------------- | ------------- | ------------- |
| CPD_SEARCH_API_KEY | string | The Azure AI Search API key |
| CPD_INSTRUMENTATION_CONNECTIONSTRING | string | The Azure ApplicationInsights connection string |
| VCS-TAG | string | The application version |
| CPD_SEARCH_BATCH_SIZE | integer (e.g. 10/20 etc) | The batch size for queries into Contentful |
| CPD_SEARCH_ENDPOINT | string | The Azure AI Search endpoint |
| CPD_SEARCH_INDEX_NAME | string | The Azure AI Search index name to access/create |
| CPD_DELIVERY_KEY | string | The Contentful delivery API key |
| CPD_CONTENTFUL_ENVIRONMENT | string | The Contentful enviroment id |
| CPD_SPACE_ID | string | The Contentful space id |
| CPD_SEARCH_RECREATE_INDEX_ON_REBUILD | boolean (true/false) | Whether to delete the index and recreate before populating |

