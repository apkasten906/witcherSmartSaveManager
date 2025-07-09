from fastapi import FastAPI

app = FastAPI()

@app.get("/")
def read_root():
    return {"message": "Witcher Save Manager API online"}
