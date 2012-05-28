module Bugfree.SharePoint.TermSetImporter

open System.IO
open System.Collections.Generic
open Microsoft.SharePoint
open Microsoft.SharePoint.Taxonomy

// See also:
// http://www.bugfree.dk/blog/2012/05/20/importing-csv-term-sets-into-sharepoint-2010-using-f/

let importCsv path =
    seq { use sr = File.OpenText(path)
          while not sr.EndOfStream do
            let line = sr.ReadLine()
            let tokens = line.Split [|','|]
            yield tokens }

let removeTermGroup (store : TermStore) name =
    store.Groups |> Seq.filter(fun g -> g.Name = name)
                 |> Seq.iter(fun g -> g.TermSets |> Seq.iter(fun t -> t.Delete())
                                      g.Delete()
                                      store.CommitAll())

let getOrCreate name children predicate create =
    match (Seq.tryFind predicate children) with
    | Some c -> c
    | None -> create name

let getOrCreateGroup name (store : TermStore) =
    getOrCreate name store.Groups (fun g -> g.Name = name)
                                  (fun name -> store.CreateGroup(name))

let getOrCreateSet name (group : Group) =
    getOrCreate name group.TermSets (fun s -> s.Name = name)
                                    (fun name -> group.CreateTermSet(name))

let getOrCreateTerm name (item : TermSetItem) =
    getOrCreate name item.Terms (fun t -> t.Name = name)
                                (fun name -> item.CreateTerm(name, 1033))

let rec importTerm (parent : TermSetItem) levels =
    match levels with
    | [] -> parent
    | head::tail -> let t = getOrCreateTerm head parent
                    importTerm t tail

let importTermSet (store : TermStore) groupName (rows : seq<string[]>) =
    let termSetName = (rows |> Seq.nth 1).[0].Replace("\"", "")
    let termSet = getOrCreateGroup groupName store |> getOrCreateSet termSetName

    rows |> Seq.skip 2
         |> Seq.iter(fun r -> r.[5..] |> Array.filter(fun i -> i <> "")
                                      |> Array.map(fun i -> i.Replace("\"", ""))
                                      |> Array.toList
                                      |> importTerm termSet
                                      |> ignore)
    store.CommitAll()

[<EntryPoint>]
let main args =
    let siteCollection = new SPSite("http://sp2010")
    let session = new TaxonomySession(siteCollection)
    let store = session.TermStores.["Managed Metadata Service"]
    let rows = importCsv "C:\Test.csv"
    let groupName = "MyGroup"
    removeTermGroup store groupName
    importTermSet store groupName rows
    0