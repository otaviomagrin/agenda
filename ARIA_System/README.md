# 🎙️ ARIA Premium System - Assistente de Voz Inteligente

## 📋 Visão Geral

ARIA é um sistema avançado de assistente de voz desenvolvido em **Visual Basic .NET** com arquitetura híbrida, múltiplas IAs e capacidades offline. Projetado para uso profissional com controle rigoroso de custos e máxima confiabilidade.

## ✨ Características Principais

### 🧠 **Sistema Multi-IA Inteligente**
- **Auto-seleção**: Testa latência e disponibilidade de 5 provedores
- **Failover automático**: Switch inteligente entre IAs
- **Modo offline**: Ollama local quando sem internet
- **Controle de custos**: Monitor de gastos com limite de $29/mês

### 🎤 **Processamento de Voz Premium**
- **STT**: AssemblyAI com diarização profissional
- **TTS**: ElevenLabs com vozes ultra-naturais
- **Wake Word**: Picovoice Porcupine ("Aria")
- **Cancelamento de ruído**: Processamento avançado

### 🏢 **Recursos Empresariais**
- **Gravação de reuniões**: Transcrição real-time com identificação de falantes
- **Resumos automáticos**: IA gera pontos-chave e ações
- **Segurança avançada**: Validação DSL e criptografia AES-256
- **Auditoria completa**: Log de todos os comandos

## 🏗️ Arquitetura

```
ARIA Premium System
├── Core (Multi-Provider AI)
│   ├── Grok xAI (Principal)
│   ├── Claude Anthropic (Análises)
│   ├── GPT-4o Mini (Balanceado)
│   ├── Gemini (Backup gratuito)
│   └── Ollama (Offline local)
├── Voice System
│   ├── AssemblyAI (STT)
│   ├── ElevenLabs (TTS)
│   └── Picovoice (Wake Word)
├── Security & Monitoring
│   ├── DSL Validator
│   ├── Health Monitor
│   └── Cost Tracker
└── Integration Layer
    ├── VB.NET Desktop
    ├── Meeting Recorder
    └── Agenda System Bridge
```

## 🚀 Instalação Rápida

### Pré-requisitos
- Windows 10+ com .NET Framework 4.8+
- 8GB RAM (16GB recomendado)
- 10GB espaço em disco
- Conexão internet estável
- GPU NVIDIA (opcional, para Ollama)

### Instalação Automática

1. **Clone o repositório**
```bash
git clone https://github.com/seu-usuario/aria-premium-system
cd ARIA_System
```

2. **Execute o instalador automático**
```bash
setup_ollama.bat
```

3. **Configure as APIs**
- Abra o aplicativo ARIA
- Vá em Configurações > APIs
- Insira suas chaves de API
- Execute o teste de conectividade

## ⚙️ Configuração de APIs

### APIs Necessárias (Custo Total: $29/mês)

| Serviço | Custo Mensal | Função | Prioridade |
|---------|--------------|--------|------------|
| xAI Grok | $5 | IA Principal | 1 |
| Claude Anthropic | $6 | Análises Complexas | 2 |
| OpenAI GPT-4o Mini | $3 | Backup Balanceado | 3 |
| AssemblyAI | $8 | Speech-to-Text | - |
| ElevenLabs | $5 | Text-to-Speech | - |
| Picovoice | $2 | Wake Word | - |
| **Total** | **$29/mês** | | |

### Configuração Manual

Edite o arquivo de configuração via interface ou manualmente:
`config/aria_config.json`

```json
{
  "ai_providers": {
    "grok": {
      "api_key": "SEU_GROK_KEY",
      "endpoint": "https://api.x.ai/v1/",
      "model": "grok-beta",
      "priority": 1
    },
    "claude": {
      "api_key": "SEU_CLAUDE_KEY",
      "endpoint": "https://api.anthropic.com/v1/",
      "model": "claude-3-sonnet-20240229",
      "priority": 2
    }
  },
  "voice": {
    "assembly_ai": {
      "api_key": "SEU_ASSEMBLY_KEY",
      "language": "pt"
    },
    "eleven_labs": {
      "api_key": "SEU_ELEVENLABS_KEY",
      "voice_id": "pNInz6obpgDQGcFmaJgB"
    }
  },
  "budget": {
    "monthly_limit": 29.00,
    "alert_threshold": 0.8
  }
}
```

## 🎯 Como Usar

### Comandos Básicos

1. **Ativar ARIA**: Diga "Aria" (wake word)
2. **Fazer pergunta**: "Aria, qual é o clima hoje?"
3. **Agendar tarefa**: "Aria, agende reunião com João amanhã às 14h"
4. **Iniciar reunião**: "Aria, inicie gravação da reunião"
5. **Resumir reunião**: "Aria, resuma os pontos principais"

### Comandos de Agenda

```bash
"Aria, quais minhas tarefas de hoje?"
"Aria, agende reunião com cliente X"
"Aria, sincronize agenda com nuvem"
"Aria, qual meu cronograma da semana?"
"Aria, crie projeto 'Website Redesign'"
"Aria, marque tarefa 'Revisar proposta' como concluída"
```

### Interface Principal

- **Status IA**: Mostra qual IA está ativa
- **Visualizador de Onda**: Animação durante fala
- **Botão Reunião**: Controle de gravação
- **Monitor de Custos**: Gasto mensal atual
- **Configurações**: Acesso rápido às opções

## 🔧 Desenvolvimento

### Estrutura do Projeto

```
ARIA_System/
├── Core/
│   ├── AriaAICore.vb          # Gerenciador principal de IA
│   ├── AIProviders/           # Implementações dos provedores
│   └── ConnectionManager.vb   # Gerenciamento de conexões
├── Voice/
│   ├── VoiceSystemPremium.vb  # Sistema de voz principal
│   ├── STTEngine.vb           # Speech-to-Text
│   └── TTSEngine.vb           # Text-to-Speech
├── Security/
│   ├── DSLValidator.vb        # Validador de comandos
│   └── SecurityManager.vb     # Gerenciamento de segurança
├── UI/
│   ├── MainForm.vb            # Interface principal
│   └── ConfigForm.vb          # Configurações
├── Utils/
│   ├── CostTracker.vb         # Rastreamento de custos
│   └── Logger.vb              # Sistema de logs
├── Meeting/
│   └── MeetingRecorderAdvanced.vb # Gravação avançada
└── Integration/
    └── AgendaSystemBridge.vb  # Ponte com sistema de agenda
```

### Compilação

```bash
# Visual Studio
Build > Build Solution (Ctrl+Shift+B)

# MSBuild (linha de comando)
msbuild ARIA_Premium_System.sln /p:Configuration=Release
```

## 🧪 Testes

Execute os testes automatizados:

```bash
cd tests
dotnet test
```

### Cenários de Teste

- ✅ Multi-IA Failover
- ✅ Modo Offline
- ✅ Controle de Custos
- ✅ Segurança de Comandos
- ✅ Gravação de Reuniões

## 📊 Monitoramento

### Health Check
- Status de todas as APIs
- Latência de conexões
- Disponibilidade de serviços

### Custos
- Gasto diário por provedor
- Projeção mensal
- Alertas de orçamento

### Logs
- Comandos executados
- Erros e exceções
- Performance metrics

## 🛡️ Segurança

- **Criptografia**: AES-256 para dados locais
- **Validação**: DSL JSON para comandos seguros
- **Auditoria**: Log completo de atividades
- **Confirmação**: Verbal para ações críticas

## 🆘 Suporte

### Problemas Comuns

**ARIA não responde ao wake word**
- Verifique permissões do microfone
- Teste a sensibilidade nas configurações

**Erro de API**
- Verifique chaves de API
- Confirme conectividade internet
- Consulte logs em `logs/aria.log`

**Alto consumo de custos**
- Revise configurações de budget
- Ative modo econômico
- Use mais o Gemini (gratuito)

### Contato

- **Issues**: GitHub Issues
- **Email**: suporte@aria-system.com
- **Documentação**: [docs.aria-system.com](https://docs.aria-system.com)

## 📄 Licença

Este projeto está licenciado sob a MIT License - veja o arquivo [LICENSE](LICENSE) para detalhes.

## 🙏 Agradecimentos

- xAI pela API Grok
- Anthropic pela API Claude
- OpenAI pela API GPT
- AssemblyAI pelo STT premium
- ElevenLabs pelo TTS natural
- Picovoice pelo wake word engine

---

**ARIA Premium System v1.0** - Desenvolvido com ❤️ em Visual Basic .NET