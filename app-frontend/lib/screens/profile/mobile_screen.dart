import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../providers/member_provider.dart';
import '../../widgets/base_button.dart';
import '../../widgets/base_text_field.dart';
import '../../widgets/linear_gradient_background.dart';
import '../../widgets/scrollable_expanded_column.dart';

class MobileScreen extends StatefulWidget {
  const MobileScreen({super.key});

  @override
  State<MobileScreen> createState() => _MobileScreenState();
}

class _MobileScreenState extends State<MobileScreen> {
  final TextEditingController _mobileController = TextEditingController();
  String? get _memberCountryCode =>
      context.read<MemberProvider>().member?.countryCode;
  String? get _memberMobile => context.read<MemberProvider>().member?.mobile;
  int _step = 1;

  @override
  void initState() {
    if (_memberMobile != null) {
      _mobileController.text = _memberCountryCode != null
          ? '+$_memberCountryCode $_memberMobile'
          : '$_memberMobile';
    } else {
      _mobileController.addListener(
        () => setState(() {}),
      );
    }
    super.initState();
  }

  @override
  void dispose() {
    _mobileController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Mobile'),
        ),
        body: ScrollableExpandedColumn(
          children: [
            BaseTextField(
              controller: _mobileController,
              labelText: "Mobile",
              hintText: 'Please enter your phone number',
              readOnly: _memberMobile != null,
            ),
            const SizedBox(height: 20),
            if (_memberMobile == null)
              BaseButton(
                onPressed: () {
                  setState(() {
                    _step = 2;
                  });
                },
                text: _step == 1 ? 'Next' : 'Save',
              ),
          ],
        ),
      ),
    );
  }
}
