# 🚀 Guia Completo de Instalação - ARIA Premium System

## 📋 Pré-requisitos do Sistema

### Requisitos Mínimos
- **Sistema Operacional**: Windows 10 (versão 1903 ou superior)
- **Framework**: .NET Framework 4.8+
- **RAM**: 8GB (16GB recomendado)
- **Armazenamento**: 10GB espaço livre
- **Internet**: Conexão estável para APIs online

### Requisitos Recomendados
- **CPU**: Intel i5/AMD Ryzen 5 ou superior
- **RAM**: 16GB+
- **GPU**: NVIDIA GTX 1060+ (para Ollama)
- **SSD**: Para melhor performance

## 🛠️ Instalação Passo a Passo

### Etapa 1: Preparação do Ambiente

1. **Verificar .NET Framework**
```bash
# Execute no PowerShell como Administrador
Get-ItemProperty "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\" -Name Release | ForEach-Object { $_.Release -ge 528040 }
```

2. **Instalar .NET Framework 4.8** (se necessário)
   - Download: https://dotnet.microsoft.com/download/dotnet-framework/net48
   - Execute como Administrador
   - Reinicie o sistema

### Etapa 2: Configuração de APIs

#### 2.1 Criar Contas nas APIs

**xAI (Grok) - $5/mês**
1. Acesse: https://x.ai/api
2. Crie conta e configure billing
3. Gere API key
4. Anote a chave: `xai-xxxxxxxxxxxxxxxxx`

**Anthropic (Claude) - $6/mês**
1. Acesse: https://console.anthropic.com
2. Crie conta e adicione créditos
3. Gere API key
4. Anote a chave: `sk-ant-xxxxxxxxxxxxxxxxx`

**OpenAI (GPT-4o Mini) - $3/mês**
1. Acesse: https://platform.openai.com
2. Crie conta e adicione billing
3. Gere API key
4. Anote a chave: `sk-xxxxxxxxxxxxxxxxx`

**Google AI (Gemini) - Gratuito**
1. Acesse: https://makersuite.google.com
2. Crie projeto no Google Cloud
3. Ative a API do Gemini
4. Gere API key
5. Anote a chave: `AIzaSyxxxxxxxxxxxxxxxxx`

**AssemblyAI (STT) - $8/mês**
1. Acesse: https://www.assemblyai.com
2. Crie conta e configure billing
3. Gere API key
4. Anote a chave: `xxxxxxxxxxxxxxxxx`

**ElevenLabs (TTS) - $5/mês**
1. Acesse: https://elevenlabs.io
2. Crie conta e escolha plano
3. Gere API key
4. Anote a chave: `xxxxxxxxxxxxxxxxx`

**Picovoice (Wake Word) - $2/mês**
1. Acesse: https://picovoice.ai
2. Crie conta gratuita
3. Gere access key
4. Anote a chave: `xxxxxxxxxxxxxxxxx`

#### 2.2 Configurar Ollama (Offline)

1. **Baixar Ollama**
```bash
# Download: https://ollama.ai/download
# Instale o Ollama para Windows
```

2. **Executar Setup Automático**
```bash
cd ARIA_System
setup_ollama.bat
```

### Etapa 3: Instalação do ARIA

#### 3.1 Compilar o Projeto

**Via Visual Studio:**
1. Abra `ARIA_Premium_System.sln`
2. Build > Build Solution (Ctrl+Shift+B)
3. Verifique se compilou sem erros

**Via MSBuild (Linha de Comando):**
```bash
cd ARIA_System
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" ARIA_Premium_System.sln /p:Configuration=Release
```

#### 3.2 Configuração Inicial

1. **Execute o ARIA pela primeira vez**
```bash
cd bin\Release
ARIA_Premium_System.exe
```

2. **Configure APIs na Interface**
   - Vá em `Configurações > APIs`
   - Insira todas as chaves de API
   - Clique em `Testar Conectividade`
   - Salve as configurações

### Etapa 4: Configuração Avançada

#### 4.1 Configuração de Voz

1. **Dispositivos de Áudio**
   - Configurações > Voz > Dispositivos
   - Selecione microfone e alto-falantes
   - Teste os dispositivos

2. **Configurações de STT**
   - Idioma: Português (pt)
   - Modelo: Universal ou Conversational
   - Qualidade: High

3. **Configurações de TTS**
   - Voz: Selecione voz preferida
   - Velocidade: 1.0 (normal)
   - Estabilidade: 0.75

#### 4.2 Configuração de Segurança

1. **Criptografia**
   - Ative criptografia de API keys
   - Configure confirmação verbal para comandos críticos

2. **Auditoria**
   - Ative logs de auditoria
   - Configure retenção de logs (30 dias padrão)

#### 4.3 Orçamento e Custos

1. **Limite Mensal**: $29 USD
2. **Alerta**: 80% do limite ($23.20)
3. **Modo Emergência**: Switch para Gemini gratuito

### Etapa 5: Integração com Agenda

#### 5.1 Configurar Bridge

1. **Verificar Agenda System**
   - Certifique-se que o sistema Electron está rodando
   - Porta padrão: 3000

2. **Testar Integração**
```bash
# Comandos de teste
"Aria, quais minhas tarefas de hoje?"
"Aria, crie tarefa teste"
"Aria, sincronize agenda"
```

#### 5.2 Comandos Disponíveis

**Gestão de Tarefas:**
- "Aria, crie tarefa [descrição]"
- "Aria, marque tarefa [nome] como concluída"
- "Aria, liste tarefas de hoje"

**Gestão de Projetos:**
- "Aria, crie projeto [nome]"
- "Aria, adicione tarefa ao projeto [nome]"

**Reuniões:**
- "Aria, agende reunião com [pessoa] em [data]"
- "Aria, inicie gravação da reunião"

## 🔧 Configuração de Desenvolvimento

### Configurar IDE

**Visual Studio 2019/2022:**
1. Instale workload ".NET desktop development"
2. Instale extensões VB.NET
3. Configure debugger

**VS Code (alternativo):**
1. Instale extensão "Visual Basic .NET"
2. Configure OmniSharp
3. Instale .NET Core SDK

### Dependências NuGet

```xml
<packages>
  <package id="Newtonsoft.Json" version="13.0.3" />
  <package id="NAudio" version="2.2.1" />
  <package id="System.Net.Http" version="4.3.4" />
</packages>
```

## 📊 Monitoramento e Manutenção

### Logs do Sistema

**Localização dos Logs:**
- Aplicação: `logs/aria.log`
- Custos: `data/costs.json`
- Auditoria: `logs/audit/`
- Reuniões: `data/meetings/`

**Níveis de Log:**
- Debug: Informações detalhadas
- Info: Operações normais
- Warning: Problemas menores
- Error: Erros que requerem atenção
- Critical: Falhas graves

### Monitoramento de Custos

1. **Dashboard de Custos**
   - Acesse via interface principal
   - Monitore gasto diário/mensal
   - Configure alertas

2. **Relatórios**
   - Relatório mensal automatizado
   - Breakdown por provedor
   - Projeções de gastos

### Backup e Recuperação

**Arquivos Importantes:**
- `config/aria_config.json` - Configurações
- `data/costs.json` - Dados de custos
- `data/meetings/` - Gravações de reuniões
- `logs/` - Logs do sistema

**Backup Automático:**
```bash
# Script de backup (executar semanalmente)
robocopy "C:\ARIA_System\config" "D:\Backup\ARIA\config" /MIR
robocopy "C:\ARIA_System\data" "D:\Backup\ARIA\data" /MIR
```

## 🆘 Solução de Problemas

### Problemas Comuns

**1. ARIA não responde ao wake word**
```bash
Soluções:
- Verificar permissões do microfone
- Testar dispositivos de áudio
- Ajustar sensibilidade do wake word
- Verificar se Picovoice está funcionando
```

**2. Erro de API "Unauthorized"**
```bash
Soluções:
- Verificar chaves de API
- Confirmar billing das APIs
- Testar conectividade
- Verificar quotas e limites
```

**3. Alto consumo de custos**
```bash
Soluções:
- Verificar configurações de budget
- Ativar modo econômico
- Usar mais Gemini (gratuito)
- Verificar logs de uso
```

**4. Ollama não funciona offline**
```bash
Soluções:
- Verificar se Ollama está instalado
- Executar "ollama serve"
- Verificar modelos: "ollama list"
- Reinstalar modelos se necessário
```

**5. Problemas de performance**
```bash
Soluções:
- Verificar RAM disponível
- Fechar aplicações desnecessárias
- Limpar cache do ARIA
- Verificar espaço em disco
```

### Suporte Técnico

**Coletando Informações para Suporte:**
1. Versão do ARIA
2. Sistema operacional
3. Logs de erro (`logs/aria.log`)
4. Configurações (`config/aria_config.json` - SEM API keys)

**Canais de Suporte:**
- GitHub Issues: Para bugs e requests
- Email: suporte@aria-system.com
- Documentação: docs.aria-system.com

## 🎓 Próximos Passos

### Após Instalação

1. **Teste Básico**
   - Ativar wake word: "Aria"
   - Fazer pergunta simples
   - Testar integração com agenda

2. **Configuração Personalizada**
   - Ajustar voz e velocidade
   - Personalizar comandos
   - Configurar atalhos

3. **Treinamento**
   - Aprenda comandos básicos
   - Pratique com reuniões
   - Explore recursos avançados

### Recursos Avançados

1. **Comandos Personalizados**
   - Crie macros de voz
   - Configure respostas automáticas

2. **Integração Avançada**
   - API personalizada para agenda
   - Webhooks para notificações

3. **Monitoramento Empresarial**
   - Dashboard de métricas
   - Relatórios personalizados

---

**✅ Instalação Concluída!**

Agora você tem o ARIA Premium System funcionando completamente. Aproveite sua assistente de voz inteligente!