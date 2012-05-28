module Bugfree.SharePoint.TermSetImporter

open Microsoft.SharePoint.Taxonomy

val importCsv : string -> seq<string[]>
val removeTermGroup : TermStore -> string -> unit
val getOrCreate : 'b -> seq<'a> -> ('a -> bool) -> ('b -> 'a) -> 'a
val getOrCreateGroup : string -> TermStore -> Group
val getOrCreateSet : string -> Group -> TermSet
val getOrCreateTerm : string -> TermSetItem -> Term
val importTerm : TermSetItem -> string list -> TermSetItem
val importTermSet : TermStore -> string -> seq<string[]> -> unit
val main : string[] -> int