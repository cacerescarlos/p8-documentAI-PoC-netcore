import OpenAI from "openai";
const openai = new OpenAI({
    apiKey: import.meta.env.VITE_OPEN_AI_API_KEY,
    dangerouslyAllowBrowser: true,
  });

export class Assistant {
  #client;
  #model;

  constructor(model = "gpt-4o-mini", client = openai) {
    // this.#client = client;
    this.#model = model;
  }

  async chat(content, history) {
    try {
      const result = await openai.chat.completions.create({
        model: this.#model,
        messages: [...history, { content, role: "user" }],
      });

      return result.choices[0].message.content;
    } catch (error) {
      throw error;
    }
  }

}
