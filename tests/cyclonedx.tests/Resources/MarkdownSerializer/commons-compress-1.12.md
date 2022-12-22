# commons-compress (Library)
BOM urn:uuid:381cbf4c-3009-4a42-a4d0-7188e72fbfac CycloneDX 1.4 1 25/10/2022 19:46:32

BOM done with tools: CycloneDX Maven plugin 2.7.1 (OWASP Foundation),
> _group:_ **org.apache.commons** _name:_ **commons-compress** _version:_ **1.22**
>
> Apache Commons Compress software defines an API for working with compression and archive formats. These include: bzip2, gzip, pack200, lzma, xz, Snappy, traditional Unix Compress, DEFLATE, DEFLATE64, LZ4, Brotli, Zstandard and ar, cpio, jar, tar, zip, dump, 7z, arj.

Component external references: [Website](https://www.apache.org/), [BuildSystem](https://github.com/apache/commons-parent/actions), [Distribution](https://repository.apache.org/service/local/staging/deploy/maven2), [IssueTracker](https://issues.apache.org/jira/browse/COMPRESS), [MailingList](https://mail-archives.apache.org/mod_mbox/commons-user/), [Vcs](https://gitbox.apache.org/repos/asf?p=commons-compress.git),
## Components
1. Library _group:_ **com.github.luben** _name:_ **zstd-jni** _version:_ **1.5.2-5** _scope:_ Required \
   _purl:_ pkg:maven/com.github.luben/zstd-jni@1.5.2-5?type=jar
1. Library _group:_ **org.brotli** _name:_ **dec** _version:_ **0.1.2** _scope:_ Required \
   _purl:_ pkg:maven/org.brotli/dec@0.1.2?type=jar
1. Library _group:_ **org.tukaani** _name:_ **xz** _version:_ **1.9** _scope:_ Required \
   _purl:_ pkg:maven/org.tukaani/xz@1.9?type=jar
1. Library _group:_ **org.ow2.asm** _name:_ **asm** _version:_ **9.4** _scope:_ Required \
   _purl:_ pkg:maven/org.ow2.asm/asm@9.4?type=jar
1. Library _group:_ **org.osgi** _name:_ **org.osgi.core** _version:_ **6.0.0** _scope:_ Optional \
   _purl:_ pkg:maven/org.osgi/org.osgi.core@6.0.0?type=jar

## Dependencies
- pkg:maven/org.apache.commons/commons-compress@1.22?type=jar
  - pkg:maven/com.github.luben/zstd-jni@1.5.2-5?type=jar
  - pkg:maven/org.brotli/dec@0.1.2?type=jar
  - pkg:maven/org.tukaani/xz@1.9?type=jar
  - pkg:maven/org.ow2.asm/asm@9.4?type=jar
  - pkg:maven/org.osgi/org.osgi.core@6.0.0?type=jar
- pkg:maven/com.github.luben/zstd-jni@1.5.2-5?type=jar
- pkg:maven/org.brotli/dec@0.1.2?type=jar
- pkg:maven/org.tukaani/xz@1.9?type=jar
- pkg:maven/org.ow2.asm/asm@9.4?type=jar
- pkg:maven/org.osgi/org.osgi.core@6.0.0?type=jar
