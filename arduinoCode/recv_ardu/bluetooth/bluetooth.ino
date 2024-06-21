#include <SPI.h>
#include <mcp_can.h>
#include <SoftwareSerial.h>

SoftwareSerial espSerial(4, 5);  // RX, TX
SoftwareSerial BT(6, 7);
String value = "";
unsigned long __time = millis();
MCP_CAN CAN(8);

void setup() {
  Serial.begin(9600);
  espSerial.begin(115200);  // 양쪽의 보드레이트를 동일하게 설정
  BT.begin(9600);
  CAN.begin(MCP_ANY, CAN_1000KBPS, MCP_8MHZ);
  CAN.setMode(MCP_NORMAL);
}

void loop() {
  static String buffer = "";  // 버퍼를 사용하여 데이터를 저장

  espSerial.listen();
  if (espSerial.available()) {
    char c = espSerial.read();

    if (c == '\n') {
      // 개행 문자를 만나면 버퍼에 있는 데이터를 처리
      processBuffer(buffer);
      buffer = "";  // 버퍼 초기화
    } else {
      // 개행 문자가 아니면 버퍼에 문자 추가
      buffer += c;
    }
  }
  if (value.length() == 8 || value.length() != 0 && millis() - __time > 900) {
    byte data[8] = {};
    for (int i = 0; i < 8 && i < value.length(); i++) {
      data[i] = (byte)value[i];
      Serial.print(value[i]);
      Serial.print(' ');
    }
    Serial.println();
    CAN.sendMsgBuf(0x01000015, 0, 8, data);
    value = "";
  }
}

void processBuffer(String data) {
  Serial.println(data);
  if (data == "NOWIFI\r" || data == "NOSERVER\r") {
    Serial.println("changeSetting");
    changeSetting(data == "NOWIFI\r");
    return;
  }
  int index = data.indexOf(':');  // ':'을 구분자로 사용
  if (index != -1) {
    String move = data.substring(index + 1);
    if (move[0] >= 'a' && move[0] <= 'z') {
      if (value.length() == 0) __time = millis();
      value += move[0];
    }
  }
}

void changeSetting(bool mode) {
  if(mode) Serial.println("WIFI ssid:password");
  else Serial.println("Server IPAddress:port");
  BT.listen();
  __time = millis();
  while (millis() - __time < 9000) {
    if (BT.available()) {
      // : 로 구분자를 나눔
      String data = BT.readString();
      Serial.println(data);
      int index = data.indexOf(':');
      String d1 = data.substring(0, index);
      String d2 = data.substring(index + 1);
      if (mode) {
        // MEMO: WIFI 연결 실패
        Serial.println("ssid: " + d1);
        Serial.println("password: " + d2);
      } else {
        // MEMO: Server 연결 실패
        Serial.println("IPAddress: " + d1);
        Serial.println("port: " + d2);
      }
      espSerial.listen();
      espSerial.println(data);
      break;
    }
  }
  Serial.println("Return To Loop");
}