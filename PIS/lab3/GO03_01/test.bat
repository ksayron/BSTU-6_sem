@echo off
echo === Testing GO03_01 Web Server ===
echo.

echo [1] GET /A
curl -s -o NUL -w "Status: %%{http_code}\n" -X GET http://localhost:3000/A
echo.

echo [2] GET /A/B
curl -s -o NUL -w "Status: %%{http_code}\n" -X GET http://localhost:3000/A/B
echo.

echo [3] POST /A
curl -s -o NUL -w "Status: %%{http_code}\n" -X POST http://localhost:3000/A
echo.

echo [4] POST /A/B
curl -s -o NUL -w "Status: %%{http_code}\n" -X POST http://localhost:3000/A/B
echo.

echo [5] PUT /A
curl -s -o NUL -w "Status: %%{http_code}\n" -X PUT http://localhost:3000/A
echo.

echo [6] PUT /A/B
curl -s -o NUL -w "Status: %%{http_code}\n" -X PUT http://localhost:3000/A/B
echo.

echo [7] GET /unlisted
curl -s -o NUL -w "Status: %%{http_code}\n" -X GET http://localhost:3000/unlisted
echo.

echo [8] POST /unlisted
curl -s -o NUL -w "Status: %%{http_code}\n" -X POST http://localhost:3000/unlisted
echo.

echo [9] PUT /unlisted
curl -s -o NUL -w "Status: %%{http_code}\n" -X PUT http://localhost:3000/unlisted
echo.

echo === Done ===
