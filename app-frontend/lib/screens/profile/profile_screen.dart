import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../providers/member_provider.dart';
import '../../themes/app_colors.dart';
import '../../widgets/base_card.dart';
import '../../widgets/linear_gradient_background.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  @override
  void initState() {
    context.read<MemberProvider>().fetchMember();
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    final member = context.watch<MemberProvider>().member;
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Profile'),
        ),
        body: SingleChildScrollView(
          padding: const EdgeInsets.all(20),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                'Personal Information',
                style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
              ),
              const SizedBox(height: 12),
              BaseCard(
                child: Column(
                  children: [
                    ListTile(
                      title: const Text("Avatar"),
                      trailing: const Icon(Icons.chevron_right),
                      onTap: () {
                        context.push('/profile/avatar');
                      },
                    ),
                    if (member?.realName != null)
                      ListTile(
                        title: const Text("User Name"),
                        trailing: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Text(
                              member?.realName ?? '-',
                              style: const TextStyle(fontSize: 14),
                            ),
                            const SizedBox(width: 10),
                            Icon(
                              Icons.copy,
                              size: 16,
                              color: AppColors.primaryColor,
                            ),
                          ],
                        ),
                        onTap: () {
                          Clipboard.setData(
                            ClipboardData(text: member?.realName ?? ''),
                          );
                          ScaffoldMessenger.of(context).showSnackBar(
                            const SnackBar(
                              content: Text('Copied to clipboard'),
                            ),
                          );
                        },
                      ),
                    ListTile(
                      title: const Text("User ID"),
                      trailing: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Text(
                            member?.uid ?? '-',
                            style: const TextStyle(fontSize: 14),
                          ),
                          const SizedBox(width: 10),
                          Icon(
                            Icons.copy,
                            size: 16,
                            color: AppColors.primaryColor,
                          ),
                        ],
                      ),
                      onTap: () {
                        Clipboard.setData(
                          ClipboardData(text: member?.uid ?? ''),
                        );
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(
                            content: Text('Copied to clipboard'),
                          ),
                        );
                      },
                    ),
                    if (member?.realName == null)
                      ListTile(
                        title: const Text("Name"),
                        trailing: const Icon(Icons.chevron_right),
                        onTap: () {
                          context.push('/profile/username');
                        },
                      ),
                  ],
                ),
              ),
              const SizedBox(height: 20),
              const Text(
                'Account Information',
                style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
              ),
              const SizedBox(height: 12),
              BaseCard(
                child: Column(
                  children: [
                    ListTile(
                      title: const Text("E-mail"),
                      trailing: const Icon(Icons.chevron_right),
                      onTap: () => context.push('/profile/email'),
                    ),
                    ListTile(
                      title: const Text("Mobile"),
                      trailing: const Icon(Icons.chevron_right),
                      onTap: () => context.push('/profile/mobile'),
                    ),
                    ListTile(
                      title: const Text("Login Password"),
                      trailing: const Icon(Icons.chevron_right),
                      onTap: () => context.push('/profile/login-password'),
                    ),
                    if (member?.fundPasswordStatus == "0")
                      ListTile(
                        title: const Text("Create Fund Password"),
                        trailing: const Icon(Icons.chevron_right),
                        onTap: () => context.push(
                          '/profile/create-fund-password',
                        ),
                      ),
                    if (member?.fundPasswordStatus == "1")
                      ListTile(
                        title: const Text("Change Fund Password"),
                        trailing: const Icon(Icons.chevron_right),
                        onTap: () => context.push(
                          '/profile/change-fund-password',
                        ),
                      ),
                    ListTile(
                      title: const Text("Crypto Address"),
                      trailing: const Icon(Icons.chevron_right),
                      onTap: () => context.push('/profile/crypto-address'),
                    ),
                    ListTile(
                      title: const Text("Card"),
                      trailing: const Icon(Icons.chevron_right),
                      onTap: () => context.push('/profile/bank-card'),
                    ),
                    ListTile(
                      title: const Text("ID Information"),
                      trailing: const Icon(Icons.chevron_right),
                      onTap: () => context.push('/profile/id-information'),
                    ),
                    if (member?.is2faEnable == "0")
                      ListTile(
                        title: const Text("Google Authentication"),
                        trailing: const Icon(Icons.chevron_right),
                        onTap: () =>
                            context.push('/profile/google-authentication'),
                      ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
