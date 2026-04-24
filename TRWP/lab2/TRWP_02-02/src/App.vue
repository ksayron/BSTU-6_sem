<template>
  <div class="container">
    <h1>TDWA02-02 SPA (Vue)</h1>

    <div class="form">
      <label>Operation:</label>
      <input v-model="op" placeholder="add | sub | mul | div" />

      <label>X:</label>
      <input type="number" v-model.number="x" />

      <label>Y:</label>
      <input type="number" v-model.number="y" />
    </div>

    <div class="buttons">
      <button @click="sendGet">GET</button>
      <button @click="sendPost">POST</button>
      <button @click="sendPut">PUT</button>
      <button @click="sendDelete">DELETE</button>
    </div>

    <h2>Result:</h2>
    <pre>{{ result }}</pre>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const baseUrl = 'https://localhost:20443/api/Save-JSON'

const op = ref('')
const x = ref(0)
const y = ref(0)
const result = ref('')

function show(data :any) {
  result.value = JSON.stringify(data, null, 2)
}

async function sendGet() {
  const res = await fetch(baseUrl)
  const text = await res.text()
  show(text)
}

async function sendPost() {
  const res = await fetch(baseUrl, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ op: op.value, x: x.value, y: y.value })
  })
  show(await res.json())
}

async function sendPut() {
  const res = await fetch(baseUrl, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ op: op.value, x: x.value, y: y.value })
  })
  show(await res.json())
}

async function sendDelete() {
  const res = await fetch(baseUrl, {
    method: 'DELETE'
  })
  show(await res.text())
}
</script>

<style>
.container {
  font-family: Arial;
  padding: 40px;

}

.form {
  background: white;
  padding: 20px;
  border-radius: 10px;
  width: 300px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.form label {
  display: block;
  margin-top: 10px;
  font-weight: bold;
}

.form input {
  width: 100%;
  padding: 6px;
  margin-top: 5px;
}

.buttons {
  margin-top: 20px;
}

button {
  padding: 8px 14px;
  margin-right: 10px;
  border: none;
  background: #3498db;
  color: white;
  border-radius: 6px;
  cursor: pointer;
}

button:hover {
  background: #2980b9;
}

pre {
  background: #1e1e1e;
  color: #00ff9c;
  padding: 15px;
  border-radius: 8px;
  margin-top: 20px;
}
</style>
