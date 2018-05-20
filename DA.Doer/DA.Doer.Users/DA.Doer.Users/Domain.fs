namespace DA.Doer.Users

module AuthId = 
    
    let auth2domain (x: string) = 
        '|' |> x.LastIndexOf  |> x.Substring

    let domain2auth = 
        sprintf "doer|%s"

