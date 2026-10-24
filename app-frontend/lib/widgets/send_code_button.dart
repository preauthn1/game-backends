import 'dart:async';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../providers/auth_provider.dart';
import 'base_button.dart';

class SendCodeButton extends StatefulWidget {
  final String countryId;
  final String mobile;

  const SendCodeButton({
    super.key,
    required this.countryId,
    required this.mobile,
  });

  @override
  State<SendCodeButton> createState() => _SendCodeButtonState();
}

class _SendCodeButtonState extends State<SendCodeButton> {
  bool _sending = false;
  int _remainingSeconds = 0;
  Timer? _timer;

  Future<void> _send() async {
    setState(() {
      _sending = true;
    });
    try {
      await context.read<AuthProvider>().fetchSmsCode(
            countryId: widget.countryId,
            mobile: widget.mobile,
          );
      _startCountdown();
    } finally {
      setState(() {
        _sending = false;
      });
    }
  }

  void _startCountdown() {
    setState(() {
      _remainingSeconds = 60;
    });

    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      setState(() {
        if (_remainingSeconds > 0) {
          _remainingSeconds--;
        } else {
          _timer?.cancel();
        }
      });
    });
  }

  @override
  Widget build(BuildContext context) {
    return BaseButton.small(
      loading: _sending,
      onPressed: _remainingSeconds > 0 ||
              widget.mobile.isEmpty ||
              widget.countryId.isEmpty
          ? null
          : _send,
      text: _remainingSeconds > 0 ? '$_remainingSeconds' : 'Send',
    );
  }
}
