import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../providers/member_provider.dart';
import '../../widgets/base_button.dart';
import '../../widgets/base_dialog.dart';
import '../../widgets/base_text_field.dart';
import '../../widgets/linear_gradient_background.dart';
import '../../widgets/scrollable_expanded_column.dart';

class EmailScreen extends StatefulWidget {
  const EmailScreen({super.key});

  @override
  State<EmailScreen> createState() => _EmailScreenState();
}

class _EmailScreenState extends State<EmailScreen> {
  final TextEditingController _emailController = TextEditingController();
  bool _isSubmitting = false;
  bool get _canSubmit => _emailController.text.isNotEmpty;
  String? get _memberEmail => context.read<MemberProvider>().member?.email;

  Future<void> _submit() async {
    if (!_canSubmit) return;
    setState(() {
      _isSubmitting = true;
    });

    try {
      final memberProvider = context.read<MemberProvider>();
      final email = _emailController.text.trim();
      await memberProvider.setEmail(email);
      if (!mounted) return;
      await showDialog(
        context: context,
        builder: (context) {
          return BaseDialog(
            titleText: "Email updated",
            message: 'Your email has been successfully updated',
            onConfirm: () {
              Navigator.of(context).pop();
            },
          );
        },
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
    if (_memberEmail != null) {
      _emailController.text = _memberEmail!;
    } else {
      _emailController.addListener(
        () => setState(() {}),
      );
    }
    super.initState();
  }

  @override
  void dispose() {
    _emailController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Email'),
        ),
        body: ScrollableExpandedColumn(
          children: [
            BaseTextField(
              controller: _emailController,
              labelText: "Email",
              hintText: 'Please enter email',
              readOnly: _memberEmail != null,
            ),
            const SizedBox(height: 20),
            if (_memberEmail == null)
              BaseButton(
                loading: _isSubmitting,
                onPressed: _canSubmit ? _submit : null,
                text: 'Save',
              ),
          ],
        ),
      ),
    );
  }
}
