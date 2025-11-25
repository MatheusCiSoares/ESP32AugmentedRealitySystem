
# ESP32 Augmented Reality System

  

Um sistema de realidade aumentada DIY utilizando ESP32 e Godot Engine, criando um protótipo de óculos AR funcionais com rastreamento de movimento em tempo real. 

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Platform](https://img.shields.io/badge/platform-ESP32-green.svg)
![Engine](https://img.shields.io/badge/engine-Godot-478cbf.svg)

  ## 📋 Índice

  
- [Sobre o Projeto](#sobre-o-projeto)

- [Arquitetura](#arquitetura)

- [Componentes](#componentes)

- [Como Funciona](#como-funciona)

- [Instalação](#instalação)

- [Configuração](#configuração)

- [Uso](#uso)

- [Estrutura do Projeto](#estrutura-do-projeto)

- [Contribuindo](#contribuindo)

- [Licença](#licença)

  

## 🎯 Sobre o Projeto

  

Este projeto implementa um sistema completo de realidade aumentada utilizando hardware acessível e tecnologias open-source. O sistema consiste em um cliente ESP32 que funciona como óculos AR e um servidor Godot que processa e renderiza o ambiente 3D.

### Características Principais
 
- ✅ Displays duplos para visão estereoscópica
- ✅ Comunicação em tempo real via WiFi
- ✅ Renderização 3D no servidor Godot
- ✅ Rastreamento de movimento 6DOF (6 graus de liberdade)
- ✅ Sistema modular e expansível


  

## 🏗️ Arquitetura

  

O sistema opera em uma arquitetura cliente-servidor:

## 🔧 Componentes

  

### Hardware Necessário

  

#### Cliente ESP32

-  **1x ESP32 WROOM**

-  **1x MPU6050** - Acelerômetro/Giroscópio 6 eixos

-  **2x Display GC9A01** - Displays circulares 240x240 pixels

-  **Cabos e protoboard** para conexões

-  **Cabo micro-usb** para alimentação da placa

  

#### Servidor

-  **Computador** com Godot Engine instalado

-  **Conexão WiFi** na mesma rede do ESP32

  

### Software Necessário

  
-  **Arduino IDE** ou **PlatformIO** para programação do ESP32

-  **Godot Engine** (versão recomendada: 4.3 Mono)

-  **Bibliotecas Arduino:**
		
- SPI
- TFT_eSPI **modificada**
- TJpg_Decoder
- WiFi (incluída no core ESP32)
- Adafruit_MPU6050
- Adafruit_Sensor
- Wire

  

## ⚙️ Como Funciona

  
### Fluxo de Operação
  

1.  **Captura de Movimento:**

- O MPU6050 conectado ao ESP32 captura dados de aceleração e rotação

- Os dados são processados e enviados via WiFi ao servidor Godot
 

2.  **Processamento no Servidor:**

- O servidor Godot recebe os dados de movimento

- Aplica a rotação/posição a uma câmera virtual no ambiente 3D

- Renderiza duas perspectivas (olho esquerdo e direito) para visão estereoscópica

 
3.  **Transmissão de Imagens:**

- As imagens renderizadas são codificadas e enviadas de volta ao ESP32

- O ESP32 recebe e decodifica as imagens

  
4.  **Exibição:**

- Cada display GC9A01 exibe a perspectiva correspondente

- O ciclo se repete continuamente para criar a experiência AR em tempo real

  

## 🚀 Instalação

### Conexões do Hardware

 
**MPU6050:**

- VCC → 3.3V

- GND → GND

- SCL → GPIO 22

- SDA → GPIO 21

 
**Display GC9A01 (Olho Esquerdo):**

- VCC → 3.3V

- GND → GND

- SCL → GPIO 18

- SDA → GPIO 23

- RES → GPIO D5

- DC → GPIO 16

- CS → RX2


  

**Display GC9A01 (Olho Direito):**

- VCC → 3.3V

- GND → GND

- SCL → GPIO 18

- SDA → GPIO 23

- RES → GPIO D5

- DC → GPIO 16

- CS → TX2

  

## 📱 Uso

  

1.  **Inicie o Servidor Godot:**

- Abra o projeto no Godot Engine

- Execute a cena principal (F5)

- Aguarde o servidor iniciar e mostrar o IP

  

2.  **Configure o ESP32:**

- Atualize o código com o IP do servidor

- Faça upload do código para o ESP32

  

3.  **Teste o Sistema:**

- Ligue o ESP32

- Aguarde a conexão WiFi ser estabelecida

- Mova os óculos e observe a resposta nos displays

 

## 🐛 Problemas Conhecidos

  
- Alta interferência no MPU6050 durante a exibição de imagens

- Imagens precisam ser mais compactadas para atingir taxas de quadros melhores

  

## 📄 Licença

  

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

  

## 👤 Autor

  

**Matheus Ci Soares**

  

- GitHub: [@MatheusCiSoares](https://github.com/MatheusCiSoares)


