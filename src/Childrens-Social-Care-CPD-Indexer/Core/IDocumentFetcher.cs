﻿namespace Childrens_Social_Care_CPD_Indexer.Core;

public interface IDocumentFetcher
{
    Task<CpdDocument[]> FetchBatchAsync(int limit, int skip, CancellationToken cancellationToken);
}