# server-picker-x

A cross platform CS2 and Deadlock server picker built with AvaloniaUI

#### To Do
##### Models
- [x] ServerModel (Partial class, implements ObservableObject from MVVM Community Toolkit)
- [x] RelayModel

#### View Models
- [x] MainWindowViewModel

#### Features
- Server List DataGrid
  - [x] Fetch and display data
  - [x] Ping one or all servers (RelayCommand)
  - [x] Server Flag Image (with binding ImageConverter (path to BitMap))
  - [x] Custom Ping Column Sorter
- Main Functionality
  - [ ] Block Selected (RelayCommand)
  - [ ] Block All (RelayCommand)
  - [ ] Unblock Selected (RelayCommand)
  - [ ] Unblock All (RelayCommand)
- [ ] Clusters
  - [ ] Create ClusterModel (server model collection property)
- [ ] Update Checker
- [ ] Presets
- [ ] Settings
  - [ ] Toggle update checker
  - [ ] Reset firewall
