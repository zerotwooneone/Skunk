
## Running

Start the database

>  docker run --name skunk-postgres -e POSTGRES_PASSWORD=secretpassword -d -p 5432:5432 -v postgres_skunk:/var/lib/postgresql/data postgres

### Running Backend

You must create a Config/secrets.json file similar to the following (based on Skunk.Postgres.Interfaces.IPostGresConfig)

```
postgres:{
    "Host": "your db server",
    "UserName": "someUsername", 
    "Password": "some Password",
    "Port": 5432 
}
```

## Troubleshooting

List processes: get container id
>  docker ps

Open terminal in container

> docker exec -it `{container id}` /bin/bash