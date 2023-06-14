# ksqlDB streams and tables

Both **streams** and **tables** in `ksqlDB` are defined using a SQL-like syntax and can be queried using standard SQL statements. They provide a declarative way to express the desired computations on the streaming data, enabling real-time processing and analyzying of this data.
The data in streams and tables can be transformed, filtered, joined and aggregated.

## Streams
A stream in `ksqlDB` represents an unbounded sequence of records in `ksqlDB`, where each record is an **immutable** unit of data (fact).
Streams are backed by Kafka topics and inherit their properties.

## Tables
A table in `ksqlDB` represents a **mutable** view of a stream. It is a continuously updated result set derived from one or more streams.
Tables have to define a required **key** that allows efficient retrieval of specific records based on the key value.
