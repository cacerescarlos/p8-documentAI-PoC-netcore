import { useState } from "react";
import styles from "./App.module.css";
import { Chat } from "./components/Chat/Chat";
import { Controls } from "./components/Controls/Controls";
import { Assistant } from "./assistants/openia";




function App() {
  const assistant = new Assistant();
const [messages, setMessages] = useState([]);

function addMessage(message) {
  setMessages((prevMessages) => [...prevMessages, message]);
}

async function handleContentSend(content) {
  addMessage({ content, role: "user" });

  try {
    const result = await assistant.chat(content, messages);
    addMessage({ content: result, role: "assistant" });
  } catch (error) {
    addMessage({ content: "Lo siento, ocurrio un problema inesperado", role: "system" });
  }

}

  return (
    <div className={styles.App} >
      <header className={styles.Header}>
        <img className={styles.Logo} src="/chat-bot.png" alt="chat" />
        <h2 className={styles.Title}>Mi Chat GPT</h2>
      </header>
      <div className={styles.ChatContainer} >
        <Chat messages={messages} />
      </div>
      <Controls onSend={handleContentSend}  />
    </div>
  )
}


export default App
