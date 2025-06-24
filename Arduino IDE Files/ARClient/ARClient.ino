#include <SPI.h>
#include <TFT_eSPI.h>
#include <TJpg_Decoder.h>

#include <WiFi.h>

#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>

SemaphoreHandle_t semSendReceive;
//==================================================================
const char* ssid = "Laboratorio"; // Nome da rede WiFi
const char* password = "ProjetosCPTI@TCC"; // Senha da rede WiFi
const char* host = "192.168.0.2"; // Endereço IPV4 do servidor
const uint16_t port = 8080;  // Porta que o servidor abre para acesso
unsigned long int last_time = millis();
WiFiClient client;

const int CS_DisplayL = 16;
const int CS_DisplayR = 17;
TFT_eSPI tft = TFT_eSPI();

uint8_t myID = 0;

Adafruit_MPU6050 mpu;

float mpu_offset[7];
float mpu_data[7]; // AccX, AccY, AccZ, Temp, GyrX, GyrY, GyrZ;
String mpuStr = "MPU: ";
//==================================================================
// Inicializador dos displays
void startTft() {
  pinMode(CS_DisplayL, OUTPUT); // Declarar o pino especificado como saída de dados.
  digitalWrite(CS_DisplayL, HIGH); // HIGH para impedir que o pino receba alterações.
 
  pinMode(CS_DisplayR, OUTPUT);
  digitalWrite(CS_DisplayR, HIGH);

  digitalWrite(CS_DisplayL, LOW);  // LOW para permitir que o pino receba alterações.
  digitalWrite(CS_DisplayR, LOW);
  tft.init(); // Inicia a biblioteca do display.
  tft.setTextSize(2); // Define o tamanho da fonte de texto.
  tft.setSwapBytes(true);
  tft.setRotation(1);
  tft.fillScreen(TFT_BLACK);
  digitalWrite(CS_DisplayL, HIGH);
  digitalWrite(CS_DisplayR, HIGH);
  TJpgDec.setCallback(tft_output);
}

bool tft_output(int16_t x, int16_t y, uint16_t w, uint16_t h, uint16_t* bitmap) {
  if ( y >= tft.height() ) return 0; // Limitar a decodificação à resolução y do display
  // Imprime primeiro a imagem do lado esquerdo
  digitalWrite(CS_DisplayL, LOW);
  tft.pushImage(x, y, w, h, bitmap);
  digitalWrite(CS_DisplayL, HIGH);
  // Depois a do lado direito
  digitalWrite(CS_DisplayR, LOW);
  tft.pushImage(x - 240, y, w, h, bitmap);
  digitalWrite(CS_DisplayR, HIGH);
  return 1;
}
void eraseScreens() {
  digitalWrite(CS_DisplayL, LOW);
  digitalWrite(CS_DisplayR, LOW);
  tft.fillScreen(TFT_BLACK);
  digitalWrite(CS_DisplayL, HIGH);
  digitalWrite(CS_DisplayR, HIGH);
}
void printCenter(char print_text[40], uint8_t text_size, int r, int g, int b, int cursor_y) {
  digitalWrite(CS_DisplayL, LOW);
  digitalWrite(CS_DisplayR, LOW);
  //Verifica se a quantidade de pixels da string é menor que a resolução da tela
  if (strlen(print_text) * (6 * text_size) <= 240){
    tft.setTextSize(text_size);
    tft.setTextColor(tft.color565(r, g, b));
    // (Resolução - (Qtd de caracteres * (pixels por caractere * tamanho da fonte))) / 2 + tamanho da fonte
    tft.setCursor( (int)(240 - (strlen(print_text) * (6 * text_size))) / 2 + text_size, cursor_y);
    tft.print(print_text);
  } else{
    tft.setTextSize(1);
    tft.setTextColor(tft.color565(255, 255, 255));
    tft.setCursor(90, 114);
    tft.print("Limite de Texto Excedido.");
  }
  digitalWrite(CS_DisplayL, HIGH);
  digitalWrite(CS_DisplayR, HIGH);
}
// Inicializa o MPU6050
void startMpu(){
  eraseScreens();
  printCenter("Conectando MPU", 2, 255, 255, 255, 114);
  Serial.println("Conectando MPU6050.");
  // Tenta iniciar o MPU
  if (!mpu.begin()) {
    eraseScreens();
    printCenter("MPU", 2, 255, 255, 255, 114);
    printCenter("não encontrado", 2, 255, 255, 255, 132);
    Serial.println("MPU6050 não encontrado.");
    while (1) {
      delay(10);
    }
  }
  eraseScreens();
  printCenter("MPU Conectado", 2, 255, 255, 255, 114);
  Serial.println("MPU6050 Conectado.");
  mpu.setAccelerometerRange(MPU6050_RANGE_16_G);
  mpu.setGyroRange(MPU6050_RANGE_250_DEG);
  mpu.setFilterBandwidth(MPU6050_BAND_21_HZ);
  Serial.println("");
  delay(100);

  Serial.println("Calculando offsets, não mova o dispositivo.");
  printCenter("Não mova", 2, 255, 255, 255, 132);
  printCenter("o dispositivo", 2, 255, 255, 255, 150);
  int samples = 100;
  for (uint8_t i = 0; i < samples; i++) {
    sensors_event_t a, g, temp;
    mpu.getEvent(&a, &g, &temp);

    mpu_offset[0] += a.acceleration.x;
    mpu_offset[1] += a.acceleration.y;
    mpu_offset[2] += a.acceleration.z;

    mpu_offset[4] += g.gyro.x;
    mpu_offset[5] += g.gyro.y;
    mpu_offset[6] += g.gyro.z;

    delay(10);
  }

  mpu_offset[0] /= samples;
  mpu_offset[1] /= samples;
  mpu_offset[2] /= samples;

  mpu_offset[4] /= samples;
  mpu_offset[5] /= samples;
  mpu_offset[6] /= samples;

  String offsetStr = "Offset calculado: ";
  for (uint8_t i = 0; i < 7; i++){
      offsetStr += String(mpu_offset[i], 2);
      if (i < 6){
        offsetStr += "|";
      }
    }
  Serial.println(offsetStr);
}
// Conexão de rede e servidor
void startWifi() {
  eraseScreens();
  printCenter("Iniciando WiFi", 2, 255, 255, 255, 114);
  Serial.println("Iniciando WiFi.");
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED)
  {
    Serial.print(".");
    delay(500);
  }
  eraseScreens();
  printCenter("WiFi conectado", 2, 255, 255, 255, 114);
  Serial.println("");
  Serial.print("WiFi conectado com sucesso. Endereço IP:");
  Serial.println(WiFi.localIP());
  delay(1000);
}
//==================================================================
void connectToServer() {
  while (!client.connect(host, port)) {
    eraseScreens();
    printCenter("Tentando se", 2, 255, 255, 255, 114);
    printCenter("conectar ao", 2, 255, 255, 255, 132);
    printCenter("servidor", 2, 255, 255, 255, 150);
    Serial.println("Tentando se conectar ao servidor.");
    delay(200);
  }
  eraseScreens();
  printCenter("Servidor conectado", 2, 255, 255, 255, 114);
  Serial.println("Conectado.");
}

void syncDevice() {
  String messageID = "ID: ";
  messageID += myID;
  client.println(messageID);
  Serial.println("[SEND] " + messageID);
  while (myID == 0){
    String msg = client.readStringUntil('\n');
    if (msg.startsWith("Seu ID: ")){
      Serial.println("[RECEIVED] " + msg);
      myID = msg.charAt(msg.length() - 1) - '0';
      break;
    }
    else if (msg.equals("Falha")){
      client.stop();
      Serial.println("Falha ao conectar-se.");
      break;
    }
    Serial.print(".");
    delay(50);
  }
  Serial.println("Meu ID agora é: " + String(myID));
}
//==================================================================
void processMpuTask(void* param) {
  while (true) {
    if (client.connected()) {
      if (xSemaphoreTake(semSendReceive, portMAX_DELAY) == pdTRUE) {
        sensors_event_t a, g, temp;
        mpu.getEvent(&a, &g, &temp);

        mpu_data[0] = a.acceleration.x - mpu_offset[0];
        mpu_data[1] = a.acceleration.y - mpu_offset[1];
        mpu_data[2] = a.acceleration.y - mpu_offset[2];

        mpu_data[3] = 0; // Temp
        
        mpu_data[4] = g.gyro.x - mpu_offset[4];
        mpu_data[5] = g.gyro.y - mpu_offset[5];
        mpu_data[6] = g.gyro.z - mpu_offset[6];

        for (uint8_t i = 0; i < 7; i++){
          mpuStr += String(mpu_data[i], 2);
          if (i < 6){
            mpuStr += "|";
          }
        }
        mpuStr += "\n"; // Evita travamentos na leitura

        xSemaphoreGive(semSendReceive);
        Serial.println(mpuStr);
      }
    }
    vTaskDelay(10);
  }
}
void communicationTask(void* param) {
  while (true) {
    if (client.connected()) {
      if (xSemaphoreTake(semSendReceive, portMAX_DELAY) == pdTRUE) {
        client.print(mpuStr); // Envio dos dados para o servidor
        mpuStr = "MPU: "; // Reinicia a string após o envio

        delay(10);

        char header[3];

        int bytesRead = 0;
        while (bytesRead < 3) {
            if (client.available()) {
                bytesRead += client.read((uint8_t*)header + bytesRead, 3 - bytesRead);
            }
            vTaskDelay(1);
        }
        // Verifica se a mensagem recebida é uma imagem válida
        if (strncmp(header, "TFT", 3) != 0) {
          Serial.println("Header inválido");
          printCenter("Header Inválido", 2, 255, 255, 255, 84);
          printCenter("Reinicie o Óculos", 2, 255, 255, 255, 114);
          client.read();
          xSemaphoreGive(semSendReceive); // Libera o semáforo
          continue;
        }

        uint32_t imageSize = 0;
        
        client.readBytes((uint8_t*)&imageSize, 4);
        // Verifica se o tamanho da imagem está dentro do esperado
        if (imageSize == 0 || imageSize > 120000) {
          Serial.print("Tamanho de imagem inválido: ");
          Serial.println(imageSize);
          xSemaphoreGive(semSendReceive); // Libera o semáforo em caso de falha
          continue;
        }

        uint8_t* buffer = (uint8_t*)malloc(imageSize);
        // Verifica se o buffer foi alocado corretamente
        if (!buffer) {
            Serial.println("Erro ao alocar memória!");
            xSemaphoreGive(semSendReceive); // Libera o semáforo em caso de falha
            return;
        }
        
        int totalRead = 0;
        while (totalRead < imageSize) {
            if (client.available()) {
                totalRead += client.read(buffer + totalRead, imageSize - totalRead);
            }
            vTaskDelay(1);
        }
        
        TJpgDec.drawJpg(-0, 0, buffer, imageSize);
        free(buffer);
        buffer = NULL;
        xSemaphoreGive(semSendReceive); // Libera o semáforo
      }
    }
    vTaskDelay(10);
  }
}

//==================================================================
void setup() {
  Serial.begin(115200);
  startTft();
  startMpu();
  startWifi();
  connectToServer();
  syncDevice();

  // Semáforo, para evitar que as tarefas se misturem
  semSendReceive = xSemaphoreCreateBinary();
  xSemaphoreGive(semSendReceive);  // Inicialmente, o semáforo está disponível para a leitura

  // Criando as tasks
  xTaskCreate(processMpuTask, "Process MPU Task", 4096, NULL, 0, NULL);
  xTaskCreate(communicationTask, "Communication Task", 8192, NULL, 1, NULL);
}

void loop() {
}