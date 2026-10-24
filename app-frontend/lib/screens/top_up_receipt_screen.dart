import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import 'package:qr_flutter/qr_flutter.dart';

import '../providers/wallet_provider.dart';
import '../themes/app_colors.dart';
import '../utils/utils.dart';
import '../widgets/base_button.dart';
import '../widgets/base_card.dart';
import '../widgets/base_dialog.dart';
import '../widgets/base_text_field.dart';
import '../widgets/linear_gradient_background.dart';
import '../widgets/scrollable_expanded_column.dart';

class TopUpReceiptScreen extends StatefulWidget {
  final String? bankId;
  final String? orderId;
  final String? bankAccountId;
  const TopUpReceiptScreen({
    super.key,
    this.bankId,
    this.orderId,
    this.bankAccountId,
  });

  @override
  State<TopUpReceiptScreen> createState() => _TopUpReceiptScreenState();
}

class _TopUpReceiptScreenState extends State<TopUpReceiptScreen> {
  final TextEditingController _transactionNoController =
      TextEditingController();
  late Future<void> _future;
  Uint8List? _image;
  bool _isSubmitting = false;

  void _submit() async {
    if (_isSubmitting) return;
    try {
      setState(() {
        _isSubmitting = true;
      });
      final compress = await compressImage(_image!);
      final image64 = await imageBytesToBase64(compress);
      if (!mounted) return;
      await context.read<WalletProvider>().topUpUploadReceipt(
            transactionNo: _transactionNoController.text,
            orderId: widget.orderId!,
            image: image64,
          );
      if (!mounted) return;
      await showDialog(
        context: context,
        builder: (context) => BaseDialog(
          titleText: 'Success',
          message: 'Your receipt has been uploaded successfully.',
          onConfirm: () => Navigator.of(context).pop(),
        ),
      );
      if (!mounted) return;
      Navigator.of(context).pop();
    } finally {
      setState(() {
        _isSubmitting = false;
      });
    }
  }

  @override
  void initState() {
    if (widget.bankId != null && widget.bankAccountId != null) {
      _future = context.read<WalletProvider>().fetchTopUpBankAccount(
            bankId: widget.bankId!,
            bankAccountId: widget.bankAccountId!,
          );
    }
    _transactionNoController.addListener(() {
      setState(() {});
    });
    super.initState();
  }

  @override
  void dispose() {
    _transactionNoController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Deposit Receipt'),
        ),
        body: widget.bankId == null || widget.orderId == null
            ? const Center(
                child: Text('Invalid bank id or order id'),
              )
            : FutureBuilder(
                future: _future,
                builder: (context, snapshot) {
                  if (snapshot.connectionState != ConnectionState.done) {
                    return const Center(child: CircularProgressIndicator());
                  }

                  if (snapshot.hasError) {
                    return const Center(
                      child: Text('Error'),
                    );
                  }

                  final account = context.read<WalletProvider>().topUpAccount;

                  if (account == null) {
                    return const Center(
                      child: Text('Account not found'),
                    );
                  }

                  return ScrollableExpandedColumn(
                    children: [
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Text(
                            'Transfer To',
                            style: TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                          const SizedBox(height: 16),
                          if (account.bankType == "1")
                            BaseCard(
                              padding: const EdgeInsets.all(20),
                              width: double.infinity,
                              child: Column(
                                children: [
                                  Row(
                                    children: [
                                      const Text(
                                        'Bank',
                                        style: TextStyle(
                                          fontSize: 16,
                                          color: Color(0xffa3a2a4),
                                        ),
                                      ),
                                      const Spacer(),
                                      Text(account.bankName ?? '-'),
                                    ],
                                  ),
                                  const SizedBox(height: 16),
                                  Row(
                                    children: [
                                      const Text(
                                        'Cardholder',
                                        style: TextStyle(
                                          fontSize: 16,
                                          color: Color(0xffa3a2a4),
                                        ),
                                      ),
                                      const Spacer(),
                                      Text(account.accountName ?? '-'),
                                    ],
                                  ),
                                  const SizedBox(height: 16),
                                  Row(
                                    children: [
                                      const Text(
                                        'Card No',
                                        style: TextStyle(
                                          fontSize: 16,
                                          color: Color(0xffa3a2a4),
                                        ),
                                      ),
                                      const Spacer(),
                                      Text(account.accountNo ?? '-'),
                                    ],
                                  ),
                                ],
                              ),
                            ),
                          if (account.bankType == "3")
                            Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                BaseCard(
                                  padding: const EdgeInsets.symmetric(
                                    vertical: 30,
                                  ),
                                  width: double.infinity,
                                  child: Center(
                                    child: QrImageView(
                                      data: account.accountNo ?? '',
                                      version: QrVersions.auto,
                                      backgroundColor: Colors.white,
                                      size: 200,
                                    ),
                                  ),
                                ),
                                const SizedBox(height: 20),
                                Text(account.bankName ?? ''),
                                const SizedBox(height: 4),
                                BaseCard(
                                  padding: const EdgeInsets.only(left: 16),
                                  child: Row(
                                    mainAxisAlignment:
                                        MainAxisAlignment.spaceBetween,
                                    children: [
                                      Text(account.accountNo ?? ''),
                                      const Spacer(),
                                      IconButton(
                                        icon: Icon(
                                          Icons.copy,
                                          color: AppColors.primaryColor,
                                        ),
                                        onPressed: () {
                                          Clipboard.setData(
                                            ClipboardData(
                                              text: account.accountNo ?? '',
                                            ),
                                          );
                                        },
                                      ),
                                    ],
                                  ),
                                ),
                              ],
                            ),
                          const SizedBox(height: 20),
                          BaseTextField(
                            controller: _transactionNoController,
                            labelText: 'Transaction Number',
                            hintText: 'Enter your transaction number',
                          ),
                          const SizedBox(height: 20),
                          const Text(
                            "Upload Receipt",
                            style: TextStyle(
                                fontWeight: FontWeight.w600, fontSize: 16),
                          ),
                          const SizedBox(height: 12),
                          BaseCard(
                            padding: _image != null
                                ? null
                                : const EdgeInsets.all(40),
                            width: double.infinity,
                            onTap: () async {
                              final image = await galleryImagePickerToBytes();
                              setState(() {
                                _image = image;
                              });
                            },
                            child: _image != null
                                ? ClipRRect(
                                    borderRadius: BorderRadius.circular(10),
                                    child: Image.memory(
                                      _image!,
                                      fit: BoxFit.cover,
                                    ),
                                  )
                                : Column(
                                    children: [
                                      Row(
                                        mainAxisAlignment:
                                            MainAxisAlignment.center,
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
                        ],
                      ),
                      const SizedBox(height: 20),
                      BaseButton(
                        loading: _isSubmitting,
                        text: 'Upload',
                        onPressed: _transactionNoController.text.isEmpty ||
                                _image == null
                            ? null
                            : _submit,
                      ),
                    ],
                  );
                },
              ),
      ),
    );
  }
}
