import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:provider/provider.dart';

import '../providers/wallet_provider.dart';
import '../utils/utils.dart';

class BalanceText extends StatefulWidget {
  const BalanceText({super.key});

  @override
  State<BalanceText> createState() => _BalanceTextState();
}

class _BalanceTextState extends State<BalanceText> {
  bool _hideBalance = false;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        if (_hideBalance)
          for (int i = 0; i < 5; i++)
            Padding(
              padding: const EdgeInsets.only(
                top: 11,
                bottom: 11,
                right: 4,
              ),
              child: Container(
                height: 24,
                width: 24,
                decoration: BoxDecoration(
                  color: Colors.grey,
                  borderRadius: BorderRadius.circular(24),
                ),
              ),
            )
        else
          Text(
            formatNumber(
              context.watch<WalletProvider>().mainWallet?.balance ?? 0,
            ),
            style: const TextStyle(fontSize: 32),
          ),
        const SizedBox(width: 8),
        GestureDetector(
          child: SvgPicture.asset(
            'assets/images/visibility${_hideBalance ? '_off' : ''}.svg',
            height: 24,
            width: 24,
          ),
          onTap: () {
            setState(() {
              _hideBalance = !_hideBalance;
            });
          },
        ),
      ],
    );
  }
}
