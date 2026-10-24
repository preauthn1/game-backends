import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../providers/member_provider.dart';
import '../../widgets/base_button.dart';
import '../../widgets/base_text_field.dart';
import '../../widgets/linear_gradient_background.dart';
import '../../widgets/scrollable_expanded_column.dart';

class UsernameScreen extends StatefulWidget {
  const UsernameScreen({super.key});

  @override
  State<UsernameScreen> createState() => _UsernameScreenState();
}

class _UsernameScreenState extends State<UsernameScreen> {
  final TextEditingController _usernameController = TextEditingController();

  @override
  void dispose() {
    _usernameController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('User Name'),
        ),
        body: ScrollableExpandedColumn(
          children: [
            BaseTextField(
              controller: _usernameController,
              labelText: "User Name",
              hintText: 'Please enter user name',
            ),
            const SizedBox(height: 20),
            BaseButton(
              onPressed: () async {
                await context
                    .read<MemberProvider>()
                    .setRealName(_usernameController.text);
                if (!context.mounted) return;
                Navigator.of(context).pop();
              },
              text: 'Save',
            ),
          ],
        ),
      ),
    );
  }
}
