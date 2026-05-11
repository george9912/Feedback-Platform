import { useState, useRef, useEffect } from "react";
import { useMsal } from "@azure/msal-react";
import { askChat } from "../services/userService";

function ChatWidget() {
  const [open, setOpen] = useState(false);
  const [fullScreen, setFullScreen] = useState(false);
  const [messages, setMessages] = useState([]);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);

  const { instance, accounts } = useMsal();
  const account = accounts?.[0];

  const messagesEndRef = useRef(null);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, fullScreen, open]);

  const toggleChat = () => setOpen((prev) => !prev);
  const toggleFullScreen = () => setFullScreen((prev) => !prev);

  const sendMessage = async () => {
    if (!input.trim()) return;

    if (!account) {
      setMessages((prev) => [
        ...prev,
        { sender: "bot", text: "Nu ești autentificat." },
      ]);
      return;
    }

    const question = input.trim();

    setMessages((prev) => [...prev, { sender: "user", text: question }]);
    setInput("");
    setLoading(true);

    try {
      const answer = await askChat(instance, account, question);
      setMessages((prev) => [...prev, { sender: "bot", text: answer }]);
    } catch (err) {
      console.error("Chat API error:", err);
      setMessages((prev) => [
        ...prev,
        { sender: "bot", text: "Sorry, I couldn't fetch the answer." },
      ]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      
<button className="icon-button chat-launcher" onClick={toggleChat}>
  ✦
</button>


      {open && (
        <div className={`chat-window ${fullScreen ? "fullscreen" : ""}`}>
          <div className="chat-header">
            <span>Assistant</span>
            <div>
              <button className="icon-button chat-header-btn" onClick={toggleFullScreen}>
                {fullScreen ? "⧉" : "⤢"}
              </button>
              <button className="icon-button chat-header-btn" onClick={toggleChat}>
                ✖
              </button>
            </div>
          </div>

          <div className="chat-messages">
            {messages.map((m, idx) => (
              <div key={idx} className={`chat-message ${m.sender}`}>
                <strong>{m.sender === "user" ? "You: " : "Assistant: "}</strong>
                <span>{m.text}</span>
              </div>
            ))}

            {loading && <div className="chat-message bot">Assistant is typing...</div>}

            <div ref={messagesEndRef} />
          </div>

          <div className="chat-input">
            <input
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") sendMessage();
              }}
              placeholder="Type a question..."
              disabled={loading}
            />

            <button onClick={sendMessage} disabled={loading || !input.trim()}>
              Send
            </button>
          </div>
        </div>
      )}
    </>
  );
}

export default ChatWidget;