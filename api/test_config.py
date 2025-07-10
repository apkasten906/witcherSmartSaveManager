import tomllib

with open("api/settings.toml", "rb") as f:
    config = tomllib.load(f)

print(config)
