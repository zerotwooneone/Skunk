
## Running

Start the database

>  docker run -v ~/data:/data/db -p 27017:27017 skunk-mongo

### Running Backend

You must create a Config/secrets.json file similar to the following (based on Skunk.MongoDb.Interfaces.IMongoConfig)

```
mongo:{
    "Host": "your mongo db server",
    "UserName": "someUsername", //optional
    "Password": "some Password", //optional
    "Port": 27017 //optional, defaults to mongo default
}
```

## Troubleshooting

List processes: get container id
>  docker ps

Open terminal in container

> docker exec -it `{container id}` /bin/bash