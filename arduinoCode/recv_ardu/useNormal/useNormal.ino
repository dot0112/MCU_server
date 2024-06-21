#include <SPI.h>
#include <mcp_can.h>
#include <SoftwareSerial.h>

SoftwareSerial espSerial(4, 5); // RX, TX
String value= "";
unsigned long __time = millis();
MCP_CAN CAN(8);

void setup() {
  Serial.begin(9600);
  espSerial.begin(115200); // 양쪽의 보드레이트를 동일하게 설정
  CAN.begin(MCP_ANY,CAN_1000KBPS, MCP_8MHZ);
  CAN.setMode(MCP_NORMAL);
}

void loop() {
  static String buffer = ""; // 버퍼를 사용하여 데이터를 저장

espSerial.listen();
  if (espSerial.available()) {
    char c = espSerial.read();

    if (c == '\n') {
      // 개행 문자를 만나면 버퍼에 있는 데이터를 처리
      processBuffer(buffer);
      buffer = ""; // 버퍼 초기화
    } else {
      // 개행 문자가 아니면 버퍼에 문자 추가
      buffer += c;
    }
  }
  if(  value.length()==8 || value.length() != 0 && millis()-__time > 900){
    byte data[8] = {};
    for (int i = 0; i < 8 && i < value.length(); i++) {
     data[i]=(byte)value[i];
     Serial.print(value[i]);
     Serial.print(' ');
    }
    Serial.println();
    CAN.sendMsgBuf(0x01000015,0,8,data);
    value = "";
  }
}

void processBuffer(String data) {
  int index = data.indexOf(':'); // ':'을 구분자로 사용
  if (index != -1) {
    String move = data.substring(index + 1);
    if(move[0]>='a' && move[0]<='z'){
    if(value.length()==0) __time = millis();
    value += move[0];
    }
  }
}
