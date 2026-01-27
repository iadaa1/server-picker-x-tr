# server-picker-x

A cross platform CS2 and Deadlock server picker built with AvaloniaUI

#### To Do
##### Models
- [x] ServerModel (Partial class, implements ObservableObject from MVVM Community Toolkit)
- [x] RelayModel
- [ ] ClusterModel (Contains ServerModel collection and cluster Name property)

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
    - [x] Windows 
    - [x] Linux
  - [ ] Block All (RelayCommand)
    - [x] Windows 
    - [x] Linux
  - [ ] Unblock Selected (RelayCommand)
    - [x] Windows 
    - [x] Linux
  - [ ] Unblock All (RelayCommand)
    - [x] Windows 
    - [x] Linux
- [ ] Clusters
  - [ ] Create ClusterModel (server model collection property)
- [ ] Update Checker
- [ ] Presets
- [ ] Settings
  - [ ] Toggle update checker
  - [ ] Reset firewall
