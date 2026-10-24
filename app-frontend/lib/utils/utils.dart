import 'dart:convert';
import 'dart:io';
import 'dart:typed_data';
import 'package:flutter/material.dart';
import 'package:flutter_image_compress/flutter_image_compress.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';
import 'package:miner/providers/member_provider.dart';
import 'package:path_provider/path_provider.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:provider/provider.dart';
import 'package:url_launcher/url_launcher.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../models/api_error.dart';
import '../models/environment.dart';
import '../providers/auth_provider.dart';
import '../providers/miner_provider.dart';
import '../providers/promotion_provider.dart';
import '../providers/wallet_provider.dart';
import '../widgets/base_dialog.dart';
import '../platform/web_js.dart'
    if (example.invalid) '../platform/mobile_stub.dart';
import 'env_list.dart';

const secureStorage = FlutterSecureStorage();

Environment get env {
  const fromEnv = String.fromEnvironment('ENV');
  return envList.containsKey(fromEnv) ? envList[fromEnv]! : envList['dev']!;
}

Future<void> showApiErrorDialog(BuildContext context, dynamic error) async {
  final message = error is ApiError ? error.errorMessage : 'Server error';
  await showDialog(
    context: context,
    builder: (context) {
      return BaseDialog(
        titleText: "Error",
        message: message,
        onConfirm: () {
          Navigator.of(context).pop();
        },
      );
    },
  );
}

Future<void> checkIdInformation(BuildContext context) async {
  final memberProvider = context.read<MemberProvider>();
  var member = memberProvider.member;
  // 0: 等待審核, 1: 通過, 2: 拒絕
  if (member?.idInformationStatus == '0') {
    await memberProvider.fetchMember();
    if (!context.mounted) return;
    member = context.read<MemberProvider>().member;
  }

  if (member?.idInformationStatus != '1') {
    await showIdInformationDialog(
      context: context,
      status: member?.idInformationStatus,
    );
    throw 'Please complete your ID information first.';
  }
}

Future<void> showIdInformationDialog({
  required BuildContext context,
  String? status,
  bool? pushToIdInformation = true,
}) async {
  String text = 'Please complete your ID information first.';
  if (status == '0') {
    text = 'Please wait for your ID information to be verified.';
  }
  await showDialog(
    context: context,
    builder: (context) {
      return BaseDialog(
        titleText: "ID Information",
        message: text,
        onConfirm: () {
          Navigator.of(context).pop();
        },
      );
    },
  );

  if (pushToIdInformation == true && status != '0') {
    if (!context.mounted) return;
    context.push('/profile/id-information');
  }
}

Future<Uint8List?> galleryImagePickerToBytes() async {
  ImagePicker picker = ImagePicker();

  XFile? file = await picker.pickImage(
    source: ImageSource.gallery,
    imageQuality: 90,
  );

  if (file != null) return await file.readAsBytes();
  return null;
}

Uint8List imageBytes(String image) {
  final base64String = image.replaceFirst(
    RegExp(r'^data:image\/[a-zA-Z0-9\+]+;base64,'),
    '',
  );
  return base64Decode(base64String);
}

Future<String> imageToBase64(File file) async {
  final bytes = await file.readAsBytes();
  final ext = file.path.split('.').last.split('.').last;
  return 'data:image/$ext;base64,${base64Encode(bytes)}';
}

Future<String> imageBytesToBase64(
  Uint8List bytes, {
  String extension = 'jpg',
}) async {
  return 'data:image/$extension;base64,${base64Encode(bytes)}';
}

Future<Uint8List> compressImage(
  Uint8List image, {
  int targetSizeInKB = 512,
}) async {
  int quality = 90;
  Uint8List? compressedData;

  if (image.lengthInBytes / 1024 < targetSizeInKB) {
    return image;
  }
  final originalImage = await decodeImageFromList(image);
  int width = originalImage.width;
  int height = originalImage.height;

  final double scale = (width > 1000 || height > 1000)
      ? 1000 / (width > height ? width : height)
      : 1.0;
  width = (width * scale).toInt();
  height = (height * scale).toInt();

  do {
    compressedData = await FlutterImageCompress.compressWithList(
      image,
      quality: quality,
      minWidth: width,
      minHeight: height,
    );
    final int fileSize = compressedData.lengthInBytes;
    if (fileSize / 1024 < targetSizeInKB) {
      break;
    }

    quality -= 5;
  } while (true);

  return compressedData;
}

Color hexToColor(String hexString) {
  hexString = hexString.replaceAll('#', '');

  if (hexString.length == 6) {
    hexString = 'FF$hexString';
  }

  return Color(int.parse(hexString, radix: 16));
}

Future<void> launchURL(String url) async {
  final Uri uri = Uri.parse(url);
  if (!await launchUrl(uri)) {
    throw 'Could not launch $url';
  }
}

String formatNumber(dynamic input) {
  double number;
  if (input is String) {
    number = double.tryParse(input) ?? 0.0;
  } else if (input is num) {
    number = input.toDouble();
  } else {
    return '-';
  }

  // 無條件捨去
  // number = (number * 100).floorToDouble() / 100;

  // 四捨五入
  double factor = 100.0;
  number = (number * factor + 0.000001).round() / factor;

  final formatted = number.toStringAsFixed(2);
  final regExp = RegExp(r'\B(?=(\d{3})+(?!\d))');
  return formatted.replaceAllMapped(regExp, (match) => ',');
}

String getSign(dynamic value, {bool showNegative = false}) {
  final num = double.tryParse(value.toString()) ?? 0;
  if (num == 0) return '';
  if (num > 0) return '+';
  if (num < 0) return showNegative ? '-' : '';
  return '';
}

Future<void> clearProvider(BuildContext context) async {
  await context.read<AuthProvider>().clear();
  if (!context.mounted) return;
  context.read<MemberProvider>().clear();
  context.read<MinerProvider>().clear();
  context.read<WalletProvider>().clear();
  context.read<PromotionProvider>().clear();
  chatClose();
  chatClear();
}

bool isUpdateAvailable(String currentVersion, String latestVersion) {
  List<int> currentParts =
      currentVersion.split('.').map((e) => int.parse(e)).toList();
  List<int> latestParts =
      latestVersion.split('.').map((e) => int.parse(e)).toList();

  int maxLength = currentParts.length > latestParts.length
      ? currentParts.length
      : latestParts.length;

  for (int i = 0; i < maxLength; i++) {
    int currentPart = i < currentParts.length ? currentParts[i] : 0;
    int latestPart = i < latestParts.length ? latestParts[i] : 0;

    if (currentPart < latestPart) return true;
    if (currentPart > latestPart) return false;
  }

  return false;
}

Future<String> getTemporaryPath(String fileName) async {
  final directory = await getTemporaryDirectory();
  return '${directory.path}/$fileName';
}

Future<bool> checkAndRequestInstallPackagesPermission() async {
  final status = await Permission.requestInstallPackages.request();
  return status.isGranted;
}
