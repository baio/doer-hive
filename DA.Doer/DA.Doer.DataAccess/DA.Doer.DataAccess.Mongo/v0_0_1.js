db.orgs.createIndex({"name" : 1}, {"unique" : 1});
db.orgs.createIndex({"ownerEmail" : 1}, {"unique" : 1});
db.users.createIndex({"email" : 1}, {"unique" : 1});