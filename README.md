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

## Configuration
Three configuration values are required to be set in the environment:
* ``CPD_KEY_VAULT_NAME`` - the name of the keyvault to retrieve configuration from in a deployed environment
* ``CPD_CONFIG_SECTION_NAME`` - this is the key name of the root of the configuration section that stores most of the application config
* ``DEV__Application__Version`` - the application version

The remaining application configuration is stored within an IConfiguration section, named using (1) above. In a deployed environment this should be stored in Azure Key Vault.

The configuration under the root key takes the following hierarchical structure:
```
    ApplicationInsights
        ConnectionString
    Contentful
        DeliveryKey
        Environment
        SpaceId
    SearchIndexing
        ApiKey
        BatchSize
        Endpoint
        IndexName
        RecreateIndex
```


### Local developer configuration
For development, this structure can be configured in the `secrets.json` file for the project.
Also make sure you set `LOCAL_ENVIRONMENT` to true,  otherwise the application will try to initialise key vault. `LOCAL_ENVIRONMENT` is not required for any other scenario.

### Deployed environments
Key names for secrets are built from the full path to the config key, separating each level with two dash characters (--). So if `CPD_CONFIG_SECTION_NAME` is set to `DEV` then it would be:
```
DEV--ApplicationInsights--ConnectionString
DEV--ApplicationVersion
DEV--Contentful--DeliveryKey
...
```
Note this format is for **Key Vault only**.