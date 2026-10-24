import 'package:flutter/material.dart';

import '../themes/app_colors.dart';

class BaseTabBar extends StatelessWidget implements PreferredSizeWidget {
  final List<Tab> tabs;
  const BaseTabBar({super.key, required this.tabs});

  @override
  Size get preferredSize => const Size.fromHeight(kToolbarHeight);

  @override
  Widget build(BuildContext context) {
    return TabBar(
      labelColor: Colors.white,
      indicatorSize: TabBarIndicatorSize.tab,
      indicator: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
          colors: [
            AppColors.primaryColor.withOpacity(0),
            AppColors.primaryColor.withOpacity(0.1),
          ],
        ),
        border: Border(
          bottom: BorderSide(color: AppColors.primaryColor, width: 3),
        ),
      ),
      tabs: tabs,
    );
  }
}
