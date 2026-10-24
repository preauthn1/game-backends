import 'dart:convert';
import 'package:crypto/crypto.dart';
import 'package:encrypt/encrypt.dart';

import 'utils.dart';

String get md5Key => env.apiMd5Key;
String get aesKey => env.apiAesKey;
String get iv => env.apiIv;

String encrypt(String data) {
  final keyHash = md5.convert(utf8.encode(aesKey)).toString();
  final key = Key.fromUtf8(keyHash.substring(0, 32));
  final encrypter = Encrypter(AES(key, mode: AESMode.cbc, padding: 'PKCS7'));
  final encrypted = encrypter.encrypt(data, iv: IV.fromUtf8(iv));
  return encrypted.base64;
}

String decrypt(String data) {
  final keyHash = md5.convert(utf8.encode(aesKey)).toString();
  final key = Key.fromUtf8(keyHash.substring(0, 32));
  final encrypter = Encrypter(AES(key, mode: AESMode.cbc, padding: 'PKCS7'));
  final decrypted = encrypter.decrypt64(data, iv: IV.fromUtf8(iv));
  return decrypted;
}
