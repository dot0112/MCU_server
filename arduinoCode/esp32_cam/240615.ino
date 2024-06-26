// 코어 분할을 통한 작업 분할
#include "esp_camera.h"
#include <WiFi.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <unistd.h>
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"

#define CAMERA_MODEL_AI_THINKER  // Has PSRAM
#include "camera_pins.h"

// Wi-Fi 설정
const char* ssid = "";
const char* password = "";

// 서버 설정
const char* serverIP = "***.***.***.***";
const int serverPort = 0;

bool cameraMode = false, arduinoMode = false;
int wifiConnectCount = 0;

static esp_err_t stream(WiFiClient client) {
  Serial.println("stream...");
  camera_fb_t* fb = NULL;
  struct timeval _timestamp;
  esp_err_t res = ESP_OK;
  size_t _jpg_buf_len = 0;
  uint8_t* _jpg_buf = NULL;
  static int64_t last_frame = 0;
  int sock = -1;
  struct sockaddr_in server_addr;

  if (!last_frame) {
    last_frame = esp_timer_get_time();
  }

    while (true) {
      if(!client.connected()) {
        cameraMode=false;
        break;
      }
      fb = esp_camera_fb_get();  // 카메라로부터 프레임을 가져옴
      if (!fb) {
        log_e("Camera capture failed");
        Serial.println("Camera capture fail");
        res = ESP_FAIL;
        break;
      }

      _timestamp.tv_sec = fb->timestamp.tv_sec;
      _timestamp.tv_usec = fb->timestamp.tv_usec;
      if (fb->format != PIXFORMAT_JPEG) {
        bool jpeg_converted = frame2jpg(fb, 80, &_jpg_buf, &_jpg_buf_len);
        esp_camera_fb_return(fb);
        fb = NULL;
        if (!jpeg_converted) {
          log_e("JPEG compression failed");
          Serial.println("JPEG compression fail");
          res = ESP_FAIL;
          break;
        }
      } else {
        _jpg_buf_len = fb->len;
        _jpg_buf = fb->buf;
      }

      if (res == ESP_OK) {
        
          uint8_t* buf=_jpg_buf;
          size_t len = _jpg_buf_len;
          for(size_t n = 0; n<len;n=n+1024){
            if(n+1024 <len){
              client.write(buf,1024);
              buf+=1024;
            }else if(len%1024>0){
              size_t remainder = len%1024;
              client.write(buf, remainder);
            }
          }
      }

      // 프레임 정리
      if (fb) {
        esp_camera_fb_return(fb);
        fb = NULL;
        _jpg_buf = NULL;
      } else if (_jpg_buf) {
        free(_jpg_buf);
        _jpg_buf = NULL;
      }

      if (res != ESP_OK) {
        log_e("Send frame failed");
        break;
      }

      int64_t fr_end = esp_timer_get_time();
      int64_t frame_time = fr_end - last_frame;
      frame_time /= 1000;

      log_i("MJPG: %uB %ums (%.1ffps), AVG: %ums (%.1ffps)",
            (uint32_t)(_jpg_buf_len),
            (uint32_t)frame_time, 1000.0 / (uint32_t)frame_time,
            avg_frame_time, 1000.0 / avg_frame_time);
    }

  return res;
}

void sendImage(void* pvParameters) {
  WiFiClient client;
   int coreID = xPortGetCoreID();
  Serial.printf("sendImage running on core %d\n", coreID);
  while(true){
    if(!client.connected()){
      client.connect(serverIP, serverPort);
    } else {
      break;
    }
  }
  if (!cameraMode) {
      client.write("camera");
      while(true){
      String s = client.readString();
      if (s.equals("camera")) {
        cameraMode = true;
        client.write("t");
        Serial.println("camera mode setup...");
        break;
      }
      }
    }
    stream(client);
}

void recvData(void* pvParameters) {
  WiFiClient client;
   int coreID = xPortGetCoreID();
  Serial.printf("recvData running on core %d\n", coreID);
  while(true){
    if(!client.connected()){
      client.connect(serverIP, serverPort);
    } else {
      break;
    }
  }
  if(!arduinoMode){
    client.write("arduino");
    while(true){
      String s = client.readString();
      if(s.equals("Arduino")) {
        arduinoMode = true;
        client.write("t");
        Serial.println("arduino mode setup...");
        break;
      }
    }
  }
  while(true){
    if(client.available()){
      char m = client.read();
      Serial.println(String("move:") + m);
      }
    }
  }

void setup() {
  Serial.begin(115200);
  Serial.setDebugOutput(true);
  Serial.println();

  // 카메라 초기화 설정
  camera_config_t config;
  config.ledc_channel = LEDC_CHANNEL_0;
  config.ledc_timer = LEDC_TIMER_0;
  config.pin_d0 = Y2_GPIO_NUM;
  config.pin_d1 = Y3_GPIO_NUM;
  config.pin_d2 = Y4_GPIO_NUM;
  config.pin_d3 = Y5_GPIO_NUM;
  config.pin_d4 = Y6_GPIO_NUM;
  config.pin_d5 = Y7_GPIO_NUM;
  config.pin_d6 = Y8_GPIO_NUM;
  config.pin_d7 = Y9_GPIO_NUM;
  config.pin_xclk = XCLK_GPIO_NUM;
  config.pin_pclk = PCLK_GPIO_NUM;
  config.pin_vsync = VSYNC_GPIO_NUM;
  config.pin_href = HREF_GPIO_NUM;
  config.pin_sscb_sda = SIOD_GPIO_NUM;
  config.pin_sscb_scl = SIOC_GPIO_NUM;
  config.pin_pwdn = PWDN_GPIO_NUM;
  config.pin_reset = RESET_GPIO_NUM;
  config.xclk_freq_hz = 20000000;
  config.pixel_format = PIXFORMAT_JPEG;

  if (psramFound()) {
    config.frame_size = FRAMESIZE_CIF;
    config.jpeg_quality = 10;
    config.fb_count = 2;
  } else {
    config.frame_size = FRAMESIZE_CIF;
    config.jpeg_quality = 12;
    config.fb_count = 1;
  }

  esp_err_t err = esp_camera_init(&config);
  if (err != ESP_OK) {
    Serial.printf("Camera init failed with error 0x%x", err);
    esp_restart();
    return;
  }

  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
    wifiConnectCount++;
    if(wifiConnectCount >= 10) esp_restart();
  }
  Serial.println("");
  Serial.println("WiFi connected");

  xTaskCreatePinnedToCore(
    sendImage,
    "sendImage",
    8192,
    NULL,
    1,
    NULL,
    0
  );

  xTaskCreatePinnedToCore(
    recvData,
    "recvData",
    8192,
    NULL,
    1,
    NULL,
    1
  );
}




void loop() {
}