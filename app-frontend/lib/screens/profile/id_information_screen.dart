import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../providers/member_provider.dart';
import '../../themes/app_colors.dart';
import '../../utils/utils.dart';
import '../../widgets/base_button.dart';
import '../../widgets/base_card.dart';
import '../../widgets/base_dialog.dart';
import '../../widgets/linear_gradient_background.dart';

class IdInformationScreen extends StatefulWidget {
  const IdInformationScreen({super.key});

  @override
  State<IdInformationScreen> createState() => _IdInformationScreenState();
}

class _IdInformationScreenState extends State<IdInformationScreen> {
  Uint8List? _frontImage;
  Uint8List? _backImage;
  bool _submitting = false;

  Future<void> _submit() async {
    setState(() {
      _submitting = true;
    });
    try {
      String errorMessage = '';
      if (_frontImage == null) {
        errorMessage += 'Please upload the front page of your ID card.\n';
      }
      if (_backImage == null) {
        errorMessage += 'Please upload the back page of your ID card.\n';
      }
      if (errorMessage.isNotEmpty) {
        throw errorMessage;
      }
      final compressFront = await compressImage(
        _frontImage!,
        targetSizeInKB: 128,
      );
      final compressBack = await compressImage(
        _backImage!,
        targetSizeInKB: 128,
      );
      if (!mounted) return;
      final front = await imageBytesToBase64(compressFront);
      final back = await imageBytesToBase64(compressBack);
      if (!mounted) return;
      await context
          .read<MemberProvider>()
          .uploadIDImage(front: front, back: back);
      if (!mounted) return;
      await showDialog(
        context: context,
        builder: (context) => BaseDialog(
          titleText: 'Success',
          message: 'ID information uploaded successfully.',
          onConfirm: () {
            Navigator.of(context).pop();
          },
        ),
      );
      if (!mounted) return;
      Navigator.of(context).pop();
    } catch (e) {
      if (!mounted) return;
      showDialog(
        context: context,
        builder: (_) => BaseDialog(
          titleText: 'Error',
          content: Text(e.toString()),
          onConfirm: () => Navigator.pop(context),
        ),
      );
    } finally {
      setState(() {
        _submitting = false;
      });
    }
  }

  Widget _buildStatus({
    required String title,
    required String image,
    required Color titleColor,
    String? remark,
  }) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: titleColor.withOpacity(0.1),
        borderRadius: BorderRadius.circular(10),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Image.asset(
            image,
            width: 30,
            height: 30,
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const SizedBox(height: 4),
                Text(
                  title,
                  style: TextStyle(
                    color: titleColor,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                if (remark != null) ...[
                  const SizedBox(height: 8),
                  Text(remark),
                ],
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildContent() {
    final member = context.watch<MemberProvider>().member;
    if (member?.idInformationStatus == "0") {
      return _buildStatus(
        image: 'assets/images/id_pending.png',
        title: "Pending review",
        titleColor: const Color(0xFFFF9900),
      );
    }
    if (member?.idInformationStatus == "1") {
      return _buildStatus(
        image: 'assets/images/id_success.png',
        title: "Already real-name",
        titleColor: const Color(0xFF00B900),
      );
    }
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        if (member?.idInformationStatus == "2")
          _buildStatus(
            image: 'assets/images/id_failed.png',
            title: "Review failed",
            titleColor: const Color(0xFFFF3352),
            remark: member?.idInformationRemark,
          ),
        if (member?.idInformationStatus == "2") const SizedBox(height: 20),
        const Text(
          'Upload a color photo or scan of the complete front and back sides of your identification card. This should include your picture, name and date of birth.',
        ),
        const SizedBox(height: 20),
        const Text(
          "Requirements",
          style: TextStyle(fontWeight: FontWeight.w600, fontSize: 16),
        ),
        const SizedBox(height: 12),
        BaseCard(
          width: double.infinity,
          padding: const EdgeInsets.all(20),
          child: Row(
            children: [
              Expanded(
                flex: 1,
                child: Column(
                  children: [
                    Image.asset('assets/images/my/id_1.png'),
                    const SizedBox(height: 24),
                    Image.asset('assets/images/my/id_2.png'),
                  ],
                ),
              ),
              const SizedBox(width: 20),
              Image.asset('assets/images/my/id_3.png'),
              const SizedBox(width: 20),
              const Expanded(
                flex: 1,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisAlignment: MainAxisAlignment.start,
                  children: [
                    Text('Front and back in 2 files'),
                    SizedBox(height: 15),
                    Text('Fully visible'),
                    SizedBox(height: 15),
                    Text('Color image'),
                    SizedBox(height: 15),
                    Text('Cropped'),
                    SizedBox(height: 15),
                    Text('Without a background'),
                    SizedBox(height: 15),
                    Text('Not expired'),
                  ],
                ),
              ),
            ],
          ),
        ),
        const SizedBox(height: 20),
        const Text(
          "Front page",
          style: TextStyle(fontWeight: FontWeight.w600, fontSize: 16),
        ),
        const SizedBox(height: 12),
        BaseCard(
          padding: _frontImage != null ? null : const EdgeInsets.all(40),
          width: double.infinity,
          onTap: () async {
            if (_submitting) return;
            final image = await galleryImagePickerToBytes();
            setState(() {
              _frontImage = image;
            });
          },
          child: _frontImage != null
              ? ClipRRect(
                  borderRadius: BorderRadius.circular(10),
                  child: Image.memory(
                    _frontImage!,
                    fit: BoxFit.cover,
                  ),
                )
              : Column(
                  children: [
                    Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(
                          Icons.file_upload_outlined,
                          color: AppColors.primaryColor,
                        ),
                        const SizedBox(width: 8),
                        const Text(
                          'Browse file',
                          style: TextStyle(fontSize: 16),
                        ),
                      ],
                    ),
                    const SizedBox(height: 12),
                    const Text(
                      'Upload a jpeg, jpg, pdf, or .png file (with a maximum size of 10 MB).',
                      style: TextStyle(color: Colors.grey),
                      textAlign: TextAlign.center,
                    )
                  ],
                ),
        ),
        const SizedBox(height: 20),
        const Text(
          "Back page",
          style: TextStyle(fontWeight: FontWeight.w600, fontSize: 16),
        ),
        const SizedBox(height: 12),
        BaseCard(
          padding: _backImage != null ? null : const EdgeInsets.all(40),
          width: double.infinity,
          onTap: () async {
            if (_submitting) return;
            final image = await galleryImagePickerToBytes();
            setState(() {
              _backImage = image;
            });
          },
          child: _backImage != null
              ? ClipRRect(
                  borderRadius: BorderRadius.circular(10),
                  child: Image.memory(
                    _backImage!,
                    fit: BoxFit.cover,
                  ),
                )
              : Column(
                  children: [
                    Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(
                          Icons.file_upload_outlined,
                          color: AppColors.primaryColor,
                        ),
                        const SizedBox(width: 8),
                        const Text(
                          'Browse file',
                          style: TextStyle(fontSize: 16),
                        ),
                      ],
                    ),
                    const SizedBox(height: 12),
                    const Text(
                      'Upload a jpeg, jpg, pdf, or .png file (with a maximum size of 10 MB).',
                      style: TextStyle(color: Colors.grey),
                      textAlign: TextAlign.center,
                    )
                  ],
                ),
        ),
        const SizedBox(height: 20),
        BaseButton(
          loading: _submitting,
          text: 'Upload',
          onPressed: _frontImage != null && _backImage != null ? _submit : null,
        ),
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('ID Information'),
        ),
        body: SingleChildScrollView(
          padding: const EdgeInsets.all(20),
          child: _buildContent(),
        ),
      ),
    );
  }
}
