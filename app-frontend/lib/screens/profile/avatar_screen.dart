import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../models/avatar.dart';
import '../../providers/member_provider.dart';
import '../../themes/app_colors.dart';
import '../../widgets/base_button.dart';
import '../../widgets/linear_gradient_background.dart';
import '../../widgets/scrollable_expanded_column.dart';

class AvatarScreen extends StatefulWidget {
  const AvatarScreen({super.key});

  @override
  State<AvatarScreen> createState() => _AvatarScreenState();
}

class _AvatarScreenState extends State<AvatarScreen> {
  late Future<void> _future;
  Avatar? _selectedAvatar;
  final double _selectedAvatarBorderWidth = 3;

  @override
  void initState() {
    _future = _fetchData();
    super.initState();
  }

  Future<void> _fetchData() async {
    await context.read<MemberProvider>().fetchAvatarList();
    if (!mounted) return;
    _selectedAvatar = context.read<MemberProvider>().currentAvatar;
  }

  @override
  Widget build(BuildContext context) {
    return LinearGradientBackground(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Avatar'),
        ),
        body: FutureBuilder(
          future: _future,
          builder: (context, snapshot) {
            if (snapshot.connectionState != ConnectionState.done) {
              return const Center(
                child: CircularProgressIndicator(),
              );
            }

            if (snapshot.hasError) {
              return const Center(
                child: Text('Error'),
              );
            }

            final list =
                context.read<MemberProvider>().avatarList.where((element) {
              return element.image != null && element.status == "1";
            }).toList();

            return ScrollableExpandedColumn(
              children: [
                GridView.builder(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                    crossAxisCount: 4,
                    crossAxisSpacing: 10 - _selectedAvatarBorderWidth,
                    mainAxisSpacing: 10 - _selectedAvatarBorderWidth,
                  ),
                  itemCount: list.length,
                  itemBuilder: (context, index) {
                    return InkWell(
                      borderRadius: BorderRadius.circular(50),
                      onTap: () {
                        setState(() {
                          _selectedAvatar = list[index];
                        });
                      },
                      child: Container(
                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          border: Border.all(
                            color: _selectedAvatar?.id == list[index].id
                                ? AppColors.primaryColor
                                : Colors.transparent,
                            width: _selectedAvatarBorderWidth,
                          ),
                        ),
                        child: CircleAvatar(
                          backgroundImage: NetworkImage(list[index].image!),
                        ),
                      ),
                    );
                  },
                ),
                const SizedBox(height: 20),
                BaseButton(
                  onPressed: _selectedAvatar == null
                      ? null
                      : () {
                          context
                              .read<MemberProvider>()
                              .setAvatar(_selectedAvatar!);
                          Navigator.of(context).pop();
                        },
                  text: 'Save',
                ),
              ],
            );
          },
        ),
      ),
    );
  }
}
