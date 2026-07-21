#!/usr/bin/env bash
# scripts/invoke-model.sh
# Single dispatch point for all AI model invocations. Reads AI_MODEL from
# the environment (defaults to "claude" for backward compatibility) and
# delegates to the right provider CLI or API.
#
# Supported AI_MODEL values:
#   claude          Claude Code CLI (npm @anthropic-ai/claude-code)
#   openai          OpenAI API (ChatGPT models via curl + API key)
#   codex           OpenAI Codex CLI (npm @openai/codex, subscription auth)
#   deepseek        DeepSeek API (OpenAI-compatible endpoint)
#   moonshot        Moonshot/Kimi API (OpenAI-compatible endpoint)
#   openai-compat   Generic OpenAI-compatible endpoint (bring your own base URL, /chat/completions)
#   openai-responses  OpenAI-compatible responses API (bring your own base URL, /responses)
#
# Reads prompt from stdin, writes review to stdout, errors to stderr.
# Exit code reflects the underlying command's exit code.
set -euo pipefail

AI_MODEL="${AI_MODEL:-claude}"

# --- Provider: claude (Claude Code CLI, subscription auth) ---
invoke_claude() {
  # Requires CLAUDE_CODE_OAUTH_TOKEN in the environment and `claude`
  # installed globally via npm. The token is generated with `claude setup-token`.
  if [ -z "${CLAUDE_CODE_OAUTH_TOKEN:-}" ]; then
    echo "CLAUDE_CODE_OAUTH_TOKEN is not set. Set it as a repo secret (Settings > Secrets and variables > Actions) so the AI review can use Claude Code." >&2
    return 1
  fi
  claude -p --output-format text
}

# --- Provider: openai (OpenAI API, API-key auth) ---
invoke_openai() {
  # Reads OPENAI_API_KEY and OPENAI_MODEL from env.
  : "${OPENAI_API_KEY:?OPENAI_API_KEY is required for the openai provider}"
  local model="${OPENAI_MODEL:-gpt-4o}"
  local prompt
  prompt=$(cat)

  # Build JSON payload with jq for safe escaping
  local payload
  payload=$(jq -n \
    --arg model "$model" \
    --arg content "$prompt" \
    '{
      model: $model,
      messages: [{role: "user", content: $content}],
      temperature: 0.3,
      max_tokens: 4096
    }')

  local response
  response=$(curl -sf -X POST "https://api.openai.com/v1/chat/completions" \
    -H "Authorization: Bearer $OPENAI_API_KEY" \
    -H "Content-Type: application/json" \
    -d "$payload") || {
    echo "OpenAI API request failed" >&2
    return 1
  }

  # Extract content from response
  echo "$response" | jq -r '.choices[0].message.content // empty'
}

# --- Provider: codex (OpenAI Codex CLI, subscription auth) ---
invoke_codex() {
  # Codex CLI uses `codex exec` or `codex chat` for one-shot prompts.
  # Requires the Codex CLI installed (npm install -g @openai/codex) and
  # authenticated (codex login or CODEX_API_KEY).
  codex exec --raw
}

# --- Provider: deepseek (DeepSeek API, OpenAI-compatible) ---
invoke_deepseek() {
  # Reads DEEPSEEK_API_KEY from env.
  : "${DEEPSEEK_API_KEY:?DEEPSEEK_API_KEY is required for the deepseek provider}"
  local model="${DEEPSEEK_MODEL:-deepseek-chat}"
  local prompt
  prompt=$(cat)

  local payload
  payload=$(jq -n \
    --arg model "$model" \
    --arg content "$prompt" \
    '{
      model: $model,
      messages: [{role: "user", content: $content}],
      temperature: 0.3,
      max_tokens: 4096
    }')

  curl -sf -X POST "https://api.deepseek.com/chat/completions" \
    -H "Authorization: Bearer $DEEPSEEK_API_KEY" \
    -H "Content-Type: application/json" \
    -d "$payload" | jq -r '.choices[0].message.content // empty'
}

# --- Provider: moonshot (Moonshot/Kimi API, OpenAI-compatible) ---
invoke_moonshot() {
  # Reads MOONSHOT_API_KEY from env.
  : "${MOONSHOT_API_KEY:?MOONSHOT_API_KEY is required for the moonshot provider}"
  local model="${MOONSHOT_MODEL:-moonshot-v1-8k}"
  local prompt
  prompt=$(cat)

  local payload
  payload=$(jq -n \
    --arg model "$model" \
    --arg content "$prompt" \
    '{
      model: $model,
      messages: [{role: "user", content: $content}],
      temperature: 0.3,
      max_tokens: 4096
    }')

  curl -sf -X POST "https://api.moonshot.cn/v1/chat/completions" \
    -H "Authorization: Bearer $MOONSHOT_API_KEY" \
    -H "Content-Type: application/json" \
    -d "$payload" | jq -r '.choices[0].message.content // empty'
}

# --- Provider: openai-compat (generic OpenAI-compatible endpoint) ---
invoke_openai_compat() {
  # Reads OPENAI_COMPAT_BASE_URL, OPENAI_COMPAT_API_KEY, and
  # OPENAI_COMPAT_MODEL from env. Works with any OpenAI-compatible API
  # (Ollama, vLLM, LiteLLM, local models, Azure OpenAI, etc.).
  : "${OPENAI_COMPAT_BASE_URL:?OPENAI_COMPAT_BASE_URL is required for the openai-compat provider}"
  : "${OPENAI_COMPAT_API_KEY:?OPENAI_COMPAT_API_KEY is required for the openai-compat provider}"
  local model="${OPENAI_COMPAT_MODEL:-gpt-4o}"
  local prompt
  prompt=$(cat)

  local payload
  payload=$(jq -n \
    --arg model "$model" \
    --arg content "$prompt" \
    '{
      model: $model,
      messages: [{role: "user", content: $content}],
      temperature: 0.3,
      max_tokens: 4096
    }')

  curl -sf -X POST "${OPENAI_COMPAT_BASE_URL%/}/chat/completions" \
    -H "Authorization: Bearer $OPENAI_COMPAT_API_KEY" \
    -H "Content-Type: application/json" \
    -d "$payload" | jq -r '.choices[0].message.content // empty'
}

# --- Provider: openai-responses (OpenAI-compatible responses API) ---
invoke_openai_responses() {
  # Uses the /responses endpoint (not /chat/completions). Reads
  # OPENAI_RESPONSES_BASE_URL, OPENAI_RESPONSES_API_KEY, and
  # OPENAI_RESPONSES_MODEL from env.
  : "${OPENAI_RESPONSES_BASE_URL:?OPENAI_RESPONSES_BASE_URL is required for the openai-responses provider}"
  : "${OPENAI_RESPONSES_API_KEY:?OPENAI_RESPONSES_API_KEY is required for the openai-responses provider}"
  local model="${OPENAI_RESPONSES_MODEL:-gpt-5.6-sol}"
  local prompt
  prompt=$(cat)

  local payload
  payload=$(jq -n \
    --arg model "$model" \
    --arg content "$prompt" \
    '{
      model: $model,
      input: $content,
      temperature: 0.3,
      max_output_tokens: 4096
    }')

  curl -sf -X POST "${OPENAI_RESPONSES_BASE_URL%/}/responses" \
    -H "Authorization: ******" \
    -H "Content-Type: application/json" \
    -d "$payload" | jq -r '.output[0].content[0].text // empty'
}

# --- Dispatch ---
case "$AI_MODEL" in
  claude)
    invoke_claude
    ;;
  openai)
    invoke_openai
    ;;
  codex)
    invoke_codex
    ;;
  deepseek)
    invoke_deepseek
    ;;
  moonshot)
    invoke_moonshot
    ;;
  openai-compat)
    invoke_openai_compat
    ;;
  openai-responses)
    invoke_openai_responses
    ;;
  *)
    echo "Unknown AI_MODEL: $AI_MODEL. Supported: claude, openai, codex, deepseek, moonshot, openai-compat, openai-responses" >&2
    exit 1
    ;;
esac