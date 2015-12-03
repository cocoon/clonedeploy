﻿using System;
using System.Collections.Generic;
using System.IO;
using DAL;
using Helpers;

namespace BLL
{
    public class GroupBootMenu
    {
        
        public static Models.GroupBootMenu GetGroupBootMenu(int groupId)
        {
            using (var uow = new DAL.UnitOfWork())
            {
                return uow.GroupBootMenuRepository.GetFirstOrDefault(p => p.GroupId == groupId);
            }
        }

        public static bool UpdateGroupBootMenu(Models.GroupBootMenu groupBootMenu)
        {
            using (var uow = new DAL.UnitOfWork())
            {
                if (uow.GroupBootMenuRepository.Exists(x => x.GroupId == groupBootMenu.GroupId))
                {
                    groupBootMenu.Id =
                        uow.GroupBootMenuRepository.GetFirstOrDefault(
                            x => x.GroupId == groupBootMenu.GroupId).Id;
                    uow.GroupBootMenuRepository.Update(groupBootMenu, groupBootMenu.Id);
                }
                else
                    uow.GroupBootMenuRepository.Insert(groupBootMenu);

                if (!uow.Save()) return false;
              
            }

            foreach (var computer in BLL.Group.GetGroupMembers(groupBootMenu.GroupId))
            {
                var computerBootMenu = new Models.ComputerBootMenu
                {
                    ComputerId = computer.Id,
                    BiosMenu = groupBootMenu.BiosMenu,
                    Efi32Menu = groupBootMenu.Efi32Menu,
                    Efi64Menu = groupBootMenu.Efi64Menu
                };

                BLL.ComputerBootMenu.UpdateComputerBootMenu(computerBootMenu);
                BLL.ComputerBootMenu.ToggleComputerBootMenu(computer, true);
            }
           
            return true;
        }



    }
}