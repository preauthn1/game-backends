import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../platform/mobile_stub.dart';
import '../providers/app_provider.dart';
import '../providers/member_provider.dart';
import '../widgets/linear_gradient_background.dart';
import '../widgets/miner_list_view.dart';
import './home_screen.dart';
import './my_screen.dart';
import './invite_screen.dart';
import './my_miner_screen.dart';

class NavigationRoot extends StatefulWidget {
  const NavigationRoot({super.key});

  @override
  State<NavigationRoot> createState() => _NavigationRootState();
}

class _NavigationRootState extends State<NavigationRoot> {
  int _selectedIndex = 0;

  Future<void> init() async {
    final memberProvider = context.read<MemberProvider>();
    memberProvider.fetchAvatarList();
    await memberProvider.fetchMember();
    final userId = memberProvider.member?.id;
    if (userId != null) {
      setMatomoUserId(userId);
    }
  }

  @override
  void initState() {
    init();
    context.read<AppProvider>().fetchAndSetTemplate();
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    final profileImage = context.watch<MemberProvider>().member?.avatarImage;
    final username = context.watch<MemberProvider>().member?.username ?? '';
    return LinearGradientBackground(
      child: Scaffold(
        appBar: [
          AppBar(
            leading: profileImage == null
                ? null
                : Container(
                    padding: const EdgeInsets.all(8.0),
                    child: CircleAvatar(
                      backgroundImage: Image.network(profileImage).image,
                    ),
                  ),
            title: Column(
              children: [
                const Text(
                  'Welcome,',
                  style: TextStyle(fontSize: 12, color: Colors.grey),
                ),
                Text(
                  username,
                  style: const TextStyle(
                    fontWeight: FontWeight.bold,
                    fontSize: 20,
                  ),
                ),
              ],
            ),
            actions: [
              IconButton(
                onPressed: () {
                  context.push('/announcement');
                },
                icon: const Icon(Icons.notifications_outlined),
              ),
            ],
          ),
          null,
          AppBar(title: const Text('Miner')),
          AppBar(title: const Text('My Miner')),
          AppBar(
            centerTitle: false,
            leading: profileImage == null
                ? null
                : Container(
                    padding: const EdgeInsets.all(8.0),
                    child: CircleAvatar(
                      backgroundImage: Image.network(profileImage).image,
                    ),
                  ),
            titleSpacing: profileImage == null ? null : 4,
            title: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  username,
                  style: const TextStyle(
                    fontWeight: FontWeight.bold,
                    fontSize: 20,
                  ),
                ),
                Text(
                  'UID: ${context.read<MemberProvider>().member?.uid}',
                  style: const TextStyle(fontSize: 12, color: Colors.grey),
                ),
              ],
            ),
            actions: [
              IconButton(
                onPressed: () {
                  context.push('/announcement');
                },
                icon: const Icon(Icons.notifications_outlined),
              ),
            ],
          ),
        ][_selectedIndex],
        body: [
          const HomeScreen(),
          const InviteScreen(),
          const MinerListView(),
          const MyMinerScreen(),
          const MyScreen(),
        ][_selectedIndex],
        bottomNavigationBar: Container(
          decoration: const BoxDecoration(
            border: Border(
              top: BorderSide(color: Color(0xFF252526), width: 1.0),
            ),
          ),
          child: NavigationBar(
            height: 64,
            backgroundColor: const Color(0xFF0D0D0E),
            indicatorColor: Colors.transparent,
            labelBehavior: NavigationDestinationLabelBehavior.alwaysHide,
            selectedIndex: _selectedIndex,
            onDestinationSelected: (int index) {
              setState(() {
                _selectedIndex = index;
              });
            },
            destinations: [
              {'iconName': 'home', 'label': 'Home'},
              {'iconName': 'invite', 'label': 'Invite'},
              {'iconName': 'cart', 'label': 'Miner'},
              {'iconName': 'miner', 'label': 'My Miner'},
              {'iconName': 'my', 'label': 'My'},
            ].map((e) {
              return NavigationDestination(
                selectedIcon: SvgPicture.asset(
                    'assets/images/${e['iconName']}_selected.svg'),
                icon: SvgPicture.asset('assets/images/${e['iconName']}.svg'),
                label: e['label']!,
                tooltip: '',
              );
            }).toList(),
          ),
        ),
      ),
    );
  }
}
