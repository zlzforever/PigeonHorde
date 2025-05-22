``` 
cd src\TestApi
dapr run -a test-api -p 5039 \
    --dapr-http-port 5201 -c ./dapr/config.yaml -d ./dapr/components -- dotnet run
```