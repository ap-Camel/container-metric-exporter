docker run example:
```
docker run -d \
  --name=CME \
  -v /var/run/docker.sock:/var/run/docker.sock \
  aymanbacktech/cme:latest
```