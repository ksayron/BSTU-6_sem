const baseUrl = "https://localhost:20443/api/Save-JSON";

function getInputData() {
    return {
        op: document.getElementById("op").value,
        x: parseFloat(document.getElementById("x").value),
        y: parseFloat(document.getElementById("y").value)
    };
}

function showResult(data) {
    document.getElementById("result").textContent =
        JSON.stringify(data, null, 2);
}

async function sendGet() {
    const res = await fetch(baseUrl, {
        method: "GET"
    });

    const text = await res.text();
    showResult(text);
}

async function sendPost() {
    const res = await fetch(baseUrl, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(getInputData())
    });

    const data = await res.json();
    showResult(data);
}

async function sendPut() {
    const res = await fetch(baseUrl, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(getInputData())
    });

    const data = await res.json();
    showResult(data);
}

async function sendDelete() {
    const res = await fetch(baseUrl, {
        method: "DELETE"
    });

    const text = await res.text();
    showResult(text);
}