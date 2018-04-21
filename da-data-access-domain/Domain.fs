namespace DA.DataAccess.Domain

type OrgDoc = {    
    Name: string
    OwnerEmail: string
}

type UserDoc = {
    OrgId: string
    Role: string
    FirstName: string
    MidName: string 
    LastName: string
    Email: string
    Phone: string
    Ancestors: string seq
    Avatar: string
}
