﻿using BloodDonationApp.Models;
using DatabaseLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BloodDonationApp.Controllers
{
    public class AccountsController : Controller
    {
        OnlineBlooadBankDbEntities DB = new OnlineBlooadBankDbEntities();
        public ActionResult AllNewUserRequests()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["UserName"])))
            {
                return RedirectToAction("Login", "Home");
            }
            var users = DB.UserTables.Where(u => u.AccountStatusID == 1).ToList();
            return View(users);
        }
        public ActionResult UserDetail(int? id)
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["UserName"])))
            {
                return RedirectToAction("Login", "Home");
            }
            var user = DB.UserTables.Find(id);
            return View(user);
        }
        public ActionResult UserApproved(int? id)
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["UserName"])))
            {
                return RedirectToAction("Login", "Home");
            }
            var user = DB.UserTables.Find(id);
            user.AccountStatusID = 2;
            DB.Entry(user).State = System.Data.Entity.EntityState.Modified;
            DB.SaveChanges();
            return RedirectToAction("AllNewUserRequests");
        }
        public ActionResult UserRejected(int? id)
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["UserName"])))
            {
                return RedirectToAction("Login", "Home");
            }
            var user = DB.UserTables.Find(id);
            user.AccountStatusID = 3;
            DB.Entry(user).State = System.Data.Entity.EntityState.Modified;
            DB.SaveChanges();
            return RedirectToAction("AllNewUserRequests");
        }
        public ActionResult AddNewDonorByBloodBank()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["UserName"])))
            {
                return RedirectToAction("Login", "Home");
            }
            var collectbloodMV = new CollectBloodMV();
            ViewBag.CityID = new SelectList(DB.CityTables.ToList(), "CityID", "City", "0");
            ViewBag.BloodGroupID = new SelectList(DB.BloodGroupsTables.ToList(), "BloodGroupID", "BloodGroup", "0");
            ViewBag.GenderID = new SelectList(DB.GenderTables.ToList(), "GenderID", "Gender", "0");
            return View(collectbloodMV);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewDonorByBloodBank(CollectBloodMV collectBloodMV)
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["UserName"])))
            {
                return RedirectToAction("Login", "Home");
            }

            int bloodbankID = 0;
            string bloodbankid = Convert.ToString(Session["BloodBankID"]);
            int.TryParse(bloodbankid, out bloodbankID);
            var currentdate = DateTime.Now.Date;

            var currentcampaign = DB.CampaignTables.Where(c => c.CampaignDate == currentdate && c.BloodBankID == bloodbankID).FirstOrDefault();


            if (ModelState.IsValid)
            {

                using (var transaction = DB.Database.BeginTransaction())
                {
                    try
                    {
                        var checkdonor = DB.DonorTables.Where(d => d.CNIC.Trim().Replace("-", "") == collectBloodMV.DonorDetails.CNIC.Trim().Replace("-", "")).FirstOrDefault();
                        if (checkdonor == null)
                        {
                            var user = new UserTable();
                            user.UserName = collectBloodMV.DonorDetails.FullName.Trim();
                            user.Password = "12345";
                            user.EmailAddress = collectBloodMV.DonorDetails.EmailAddress;
                            user.AccountStatusID = 2;
                            user.UserTypeID = 2;
                            user.Description = "Add By Blood Bank";
                            DB.UserTables.Add(user);
                            DB.SaveChanges();

                            var donor = new DonorTable();
                            donor.FullName = collectBloodMV.DonorDetails.FullName;
                            donor.BloodGroupID = collectBloodMV.BloodGroupID;
                            donor.Location = collectBloodMV.DonorDetails.Location;
                            donor.ContactNo = collectBloodMV.DonorDetails.ContactNo;
                            donor.LastDonationDate = DateTime.Now;
                            donor.CNIC = collectBloodMV.DonorDetails.CNIC;
                            donor.GenderID = collectBloodMV.GenderID;
                            donor.CityID = collectBloodMV.CityID;
                            donor.UserID = user.UserID;
                            DB.DonorTables.Add(donor);
                            DB.SaveChanges();
                            checkdonor = DB.DonorTables.Where(d => d.CNIC.Trim().Replace("-", "") == collectBloodMV.DonorDetails.CNIC.Trim().Replace("-", "")).FirstOrDefault();

                        }

                        var checkbloodgroupstock = DB.BloodBankStockTables.Where(s => s.BloodBankID == bloodbankID && s.BloodGroupID == collectBloodMV.BloodGroupID).FirstOrDefault();
                        if (checkbloodgroupstock == null)
                        {
                            var bloodbankstock = new BloodBankStockTable();
                            bloodbankstock.BloodBankID = bloodbankID;
                            bloodbankstock.BloodGroupID = collectBloodMV.BloodGroupID;
                            bloodbankstock.Status = true;
                            bloodbankstock.Quantity = 0;
                            bloodbankstock.Description = "";
                            DB.BloodBankStockTables.Add(bloodbankstock);
                            DB.SaveChanges();
                            checkbloodgroupstock = DB.BloodBankStockTables.Where(s => s.BloodBankID == bloodbankID && s.BloodGroupID == collectBloodMV.BloodGroupID).FirstOrDefault();

                        }
                        checkbloodgroupstock.Quantity += collectBloodMV.Quantity;
                        DB.Entry(checkbloodgroupstock).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        var collectblooddetail = new BloodBankStockDetailTable();
                        collectblooddetail.BloodBankStockDetailID = checkbloodgroupstock.BloodBankStockID;
                        collectblooddetail.BloodBankStockID = collectBloodMV.BloodBankStockID;
                        collectblooddetail.BloodGroupID = collectBloodMV.BloodGroupID;
                        collectblooddetail.CampaignID = collectBloodMV.CampaignID;
                        collectblooddetail.Quantity = collectBloodMV.Quantity;
                        collectblooddetail.DonorID = collectBloodMV.DonorID;
                        collectblooddetail.DonateDateTime = DateTime.Now;
                        DB.Entry(collectblooddetail).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        transaction.Commit();

                        return RedirectToAction("BloodBankStock", "BloodBank");
                    }
                    catch
                    {
                        ModelState.AddModelError(string.Empty, "Please Provide Correct Information!");
                        transaction.Rollback();
                    }

                }

            }
            else
            {
                ModelState.AddModelError(string.Empty, "Please Provide Donor Full Details!");

            }

            ViewBag.CityID = new SelectList(DB.CityTables.ToList(), "CityID", "City", collectBloodMV.CityID);
            ViewBag.BloodGroupID = new SelectList(DB.BloodGroupsTables.ToList(), "BloodGroupID", "BloodGroup", collectBloodMV.BloodGroupID);
            ViewBag.GenderID = new SelectList(DB.GenderTables.ToList(), "GenderID", "Gender", collectBloodMV.GenderID);
            //return RedirectToAction("BloodBankStock", "BloodBank");
            return View(collectBloodMV);
        }
    }
}