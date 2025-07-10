import styles from "./Chat.module.css"
const WELCOME_MESSAGE = 
    {
      role: "assistant",
      content: "Hola! Como te puedo ayudar hoy?",
    }
  ;

export function Chat({ messages }) {
    return (
        <div className={styles.Chat} >{ [WELCOME_MESSAGE, ...messages].map(({role, content}, index)=>(
            <div className={styles.Message} key={index} data-role={role}  >
               {content}</div>
   )) }</div>
    );
}